/*
 * Base Lunar · js/game.js
 * Motor puro: estado, reglas, respaldos y antibloqueo (sección 3 del Prompt 0 y DECISIONES.md).
 * Nunca toca el DOM, la red ni el almacenamiento. Cada función devuelve un estado nuevo
 * y la lista de eventos que produjo; si la acción no corresponde a la fase, devuelve el
 * mismo estado y ningún evento (así un doble clic no aplica dos decisiones).
 */
'use strict';

window.BaseLunar = window.BaseLunar || {};

(function (BL) {
  const R = BL.config.REGLAS;
  const CLAVES = ['proteccion', 'soporte', 'bateria'];
  const FASES = Object.freeze({
    INICIO: 'INICIO',
    NIVEL: 'NIVEL',
    RESULTADO: 'RESULTADO',
    RESPALDO: 'RESPALDO',
    VICTORIA: 'VICTORIA',
    MISION_ABORTADA: 'MISION_ABORTADA',
  });

  const recortar = (v) => Math.min(R.VALOR_MAXIMO, Math.max(R.VALOR_MINIMO, v));
  const copiar = (m) => ({ proteccion: m.proteccion, soporte: m.soporte, bateria: m.bateria });
  const sinCambios = (estado) => ({ estado, eventos: [] });
  const sumar = (subida) => Object.values(subida).reduce((a, b) => a + b, 0);

  function medidoresIniciales() {
    return { proteccion: R.VALOR_INICIAL, soporte: R.VALOR_INICIAL, bateria: R.VALOR_INICIAL };
  }

  /**
   * Estado inicial: pantalla de Inicio, medidores al 80 % y 3 respaldos.
   * @returns {object}
   */
  function crearEstadoInicial() {
    return {
      fase: FASES.INICIO,
      nivel: 0,
      medidores: medidoresIniciales(),
      foto: medidoresIniciales(),
      respaldos: R.RESPALDOS_INICIALES,
      claseX: false,
      fallidos: [],
      ultimaOpcion: null,
    };
  }

  /**
   * Suma los efectos y recorta a 0–100. Falla todo medidor que quede en 0 o menos
   * antes de recortar (D-02); «al máximo» es toda ganancia que pasa de 100.
   * @returns {{medidores: object, crudos: object, fallidos: string[], alMaximo: string[]}}
   */
  function aplicarEfectos(medidores, efectos) {
    const crudos = {};
    const nuevos = {};
    CLAVES.forEach((k) => {
      crudos[k] = medidores[k] + (efectos[k] || 0);
      nuevos[k] = recortar(crudos[k]);
    });
    return {
      medidores: nuevos,
      crudos,
      fallidos: R.PRIORIDAD_FALLO.filter((k) => crudos[k] <= R.VALOR_MINIMO),
      alMaximo: CLAVES.filter((k) => (efectos[k] || 0) > 0 && crudos[k] > R.VALOR_MAXIMO),
    };
  }

  /**
   * Requisito del Nivel 5: estrictamente «mayor que» (3.4).
   * @returns {boolean}
   */
  function cumpleRequisito(medidores, requisito) {
    return medidores[requisito.medidor] > requisito.umbral;
  }

  function opcionViable(medidores, opcion) {
    if (opcion.requisito) return cumpleRequisito(medidores, opcion.requisito);
    return aplicarEfectos(medidores, opcion.efectos).fallidos.length === 0;
  }

  /**
   * ¿Hay al menos una opción que no falla (o que gana, en el Nivel 5)?
   * @returns {boolean}
   */
  function esSobrevivible(medidores, nivel) {
    return Object.values(nivel.opciones).some((opcion) => opcionViable(medidores, opcion));
  }

  /** Subida mínima por medidor para que una opción sea viable: {medidor: cantidad}. */
  function subidaNecesaria(medidores, opcion) {
    const subida = {};
    if (opcion.requisito) {
      const { medidor, umbral } = opcion.requisito;
      if (medidores[medidor] <= umbral) subida[medidor] = umbral + 1 - medidores[medidor];
      return subida;
    }
    CLAVES.forEach((k) => {
      const crudo = medidores[k] + (opcion.efectos[k] || 0);
      if (crudo <= R.VALOR_MINIMO) subida[k] = R.VALOR_MINIMO + 1 - crudo;
    });
    return subida;
  }

  /**
   * Antibloqueo (3.5, D-03): la menor ayuda que hace viable alguna opción (en empate, la A).
   * @returns {{medidores: object, ajustes: object} | null} null si no hace falta ayuda.
   */
  function calcularAyuda(medidores, nivel) {
    if (esSobrevivible(medidores, nivel)) return null;
    const subidas = Object.keys(nivel.opciones).map((id) => subidaNecesaria(medidores, nivel.opciones[id]));
    const ajustes = subidas.reduce((mejor, s) => (sumar(s) < sumar(mejor) ? s : mejor));
    const nuevos = copiar(medidores);
    Object.keys(ajustes).forEach((k) => { nuevos[k] = recortar(nuevos[k] + ajustes[k]); });
    return { medidores: nuevos, ajustes };
  }

  /** Aplica el antibloqueo si hace falta y anota el evento. */
  function conAyuda(medidores, nivel, eventos) {
    const ayuda = calcularAyuda(medidores, nivel);
    if (!ayuda) return medidores;
    eventos.push({ tipo: 'ayudaActivada', ajustes: ayuda.ajustes });
    return ayuda.medidores;
  }

  /** Entra en un nivel: penalización X (antes de la foto), antibloqueo y foto. */
  function entrarNivel(estado, numero, contexto, niveles) {
    const nivel = niveles[numero - 1];
    const claseX = Boolean(contexto.claseX);
    const eventos = [];
    let medidores = copiar(estado.medidores);
    if (nivel.penalizacionX && claseX) {
      medidores.proteccion = recortar(medidores.proteccion - R.PENALIZACION_X);
      eventos.push({ tipo: 'penalizacionX', medidor: 'proteccion', cantidad: R.PENALIZACION_X });
    }
    medidores = conAyuda(medidores, nivel, eventos);
    const nuevo = { ...estado, fase: FASES.NIVEL, nivel: numero, medidores, foto: copiar(medidores), claseX, fallidos: [], ultimaOpcion: null };
    return { estado: nuevo, eventos };
  }

  /**
   * Empieza la partida desde Inicio, en el nivel 1 o en el que pida el contexto (D-08).
   * @param {object} contexto {nivel?: number, claseX?: boolean}
   */
  function empezar(estado, contexto = {}, niveles = BL.niveles) {
    if (estado.fase !== FASES.INICIO) return sinCambios(estado);
    const pedido = Number(contexto.nivel);
    const numero = Number.isInteger(pedido) && pedido >= 1 && pedido <= niveles.length ? pedido : 1;
    return entrarNivel(crearEstadoInicial(), numero, contexto, niveles);
  }

  /** Resta un respaldo; con 0 respaldos, Misión Abortada (D-04). */
  function perderRespaldo(estado, fallidos, motivo, eventos) {
    const respaldos = estado.respaldos - 1;
    eventos.push({ tipo: 'respaldoActivado', motivo, fallidos, respaldos });
    if (respaldos > 0) return { estado: { ...estado, respaldos, fallidos, fase: FASES.RESPALDO }, eventos };
    eventos.push({ tipo: 'misionAbortada' });
    return { estado: { ...estado, respaldos: 0, fallidos, fase: FASES.MISION_ABORTADA }, eventos };
  }

  function decidirConEfectos(estado, nivel, opcionId) {
    const opcion = nivel.opciones[opcionId];
    const r = aplicarEfectos(estado.medidores, opcion.efectos);
    const eventos = [{ tipo: 'efectos', efectos: { ...opcion.efectos } }];
    r.alMaximo.forEach((k) => eventos.push({ tipo: 'medidorAlMaximo', medidor: k }));
    const base = { ...estado, medidores: r.medidores, ultimaOpcion: opcionId };
    if (r.fallidos.length) return perderRespaldo(base, r.fallidos, r.fallidos[0], eventos);
    eventos.push({ tipo: 'nivelSuperado', nivel: nivel.id });
    return { estado: { ...base, fase: FASES.RESULTADO }, eventos };
  }

  function decidirConCondicion(estado, nivel, opcionId) {
    const base = { ...estado, ultimaOpcion: opcionId };
    if (!cumpleRequisito(estado.medidores, nivel.opciones[opcionId].requisito)) {
      return perderRespaldo(base, [], 'condicion', []);
    }
    return { estado: { ...base, fase: FASES.RESULTADO }, eventos: [{ tipo: 'nivelSuperado', nivel: nivel.id }] };
  }

  /**
   * Aplica la opción elegida ('A' o 'B'). Solo actúa en la fase NIVEL.
   * Eventos posibles: efectos, medidorAlMaximo, nivelSuperado, respaldoActivado, misionAbortada.
   * @returns {{estado: object, eventos: object[]}}
   */
  function aplicarDecision(estado, opcionId, niveles = BL.niveles) {
    if (estado.fase !== FASES.NIVEL) return sinCambios(estado);
    const nivel = niveles[estado.nivel - 1];
    if (!nivel.opciones[opcionId]) return sinCambios(estado);
    if (nivel.tipo === 'condicion') return decidirConCondicion(estado, nivel, opcionId);
    return decidirConEfectos(estado, nivel, opcionId);
  }

  /**
   * Reintento (3.3, D-01): restaura la foto del nivel; cada medidor que falló sube a
   * máx(foto, 30). El estado restaurado pasa a ser la nueva foto. La penalización X
   * ya está en la foto, así que no se repite.
   * @returns {{estado: object, eventos: object[]}}
   */
  function reintentar(estado, niveles = BL.niveles) {
    if (estado.fase !== FASES.RESPALDO) return sinCambios(estado);
    const medidores = copiar(estado.foto);
    estado.fallidos.forEach((k) => { medidores[k] = Math.max(estado.foto[k], R.MINIMO_REINTENTO); });
    const eventos = [{ tipo: 'reintento', restaurados: [...estado.fallidos] }];
    const finales = conAyuda(medidores, niveles[estado.nivel - 1], eventos);
    const nuevo = { ...estado, fase: FASES.NIVEL, medidores: finales, foto: copiar(finales), fallidos: [], ultimaOpcion: null };
    return { estado: nuevo, eventos };
  }

  /**
   * Tras el resultado: siguiente nivel o, después del Nivel 5, la Victoria (D-07).
   * @param {object} contexto {claseX?: boolean} para la penalización del Nivel 4.
   */
  function continuar(estado, contexto = {}, niveles = BL.niveles) {
    if (estado.fase !== FASES.RESULTADO) return sinCambios(estado);
    if (estado.nivel < niveles.length) return entrarNivel(estado, estado.nivel + 1, contexto, niveles);
    const victoria = { ...estado, fase: FASES.VICTORIA };
    return { estado: victoria, eventos: [{ tipo: 'victoria', respaldos: estado.respaldos }] };
  }

  /**
   * «Jugar de nuevo»: devuelve exactamente el estado inicial.
   * @returns {object}
   */
  function reiniciar() {
    return crearEstadoInicial();
  }

  BL.motor = Object.freeze({
    FASES,
    crearEstadoInicial,
    empezar,
    aplicarDecision,
    reintentar,
    continuar,
    reiniciar,
    aplicarEfectos,
    cumpleRequisito,
    esSobrevivible,
    calcularAyuda,
  });
})(window.BaseLunar);
