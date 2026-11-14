/*
 * Base Lunar · js/ui.js
 * Vista: el único archivo que toca el DOM. Recibe la acción del usuario, llama al motor
 * y dibuja el nuevo estado. Escribe con textContent y crea nodos; nunca usa innerHTML.
 */
'use strict';

window.BaseLunar = window.BaseLunar || {};

(function (BL) {
  const { REGLAS, TIEMPOS } = BL.config;
  const MOTOR = BL.motor;
  const FASES = MOTOR.FASES;
  const NIVELES = BL.niveles;
  const MEDIDORES = BL.medidores;
  const TEXTOS = BL.textos;

  const VISTA_POR_FASE = Object.freeze({
    INICIO: 'vista-inicio',
    NIVEL: 'vista-nivel',
    RESULTADO: 'vista-nivel',
    RESPALDO: 'vista-nivel',
    VICTORIA: 'vista-victoria',
    MISION_ABORTADA: 'vista-abortada',
  });

  const TITULO_POR_FASE = Object.freeze({
    INICIO: 'inicio-titulo',
    NIVEL: 'nivel-titulo',
    VICTORIA: 'victoria-titulo',
    MISION_ABORTADA: 'abortada-titulo',
  });

  /** Estado de la vista. El estado del juego lo produce siempre el motor. */
  const ui = {
    estado: MOTOR.crearEstadoInicial(),
    bloqueado: false,
    ayuda: null,
    tormenta: BL.nasaData.respaldoHistorico(),
  };

  // ---------- Utilidades ----------
  const $ = (id) => document.getElementById(id);
  const plantilla = (texto, valores) => texto.replace(/\{(\w+)\}/g, (_, k) => String(valores[k]));
  const porcentaje = (v) => v + ' %';
  const conSigno = (v) => (v > 0 ? '+' : '−') + Math.abs(v);
  const medidor = (clave) => MEDIDORES.find((m) => m.clave === clave);
  const nivelActual = () => NIVELES[ui.estado.nivel - 1];
  const buscar = (eventos, tipo) => eventos.find((e) => e.tipo === tipo);
  const efectosOrdenados = (efectos) => Object.entries(efectos).sort((a, b) => b[1] - a[1]);
  const mayuscula = (texto) => texto.charAt(0).toUpperCase() + texto.slice(1);
  const movimientoReducido = () => window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  const fechaUTC = new Intl.DateTimeFormat('es', { day: 'numeric', month: 'long', year: 'numeric', timeZone: 'UTC' });
  const fechaLocal = new Intl.DateTimeFormat('es', { day: 'numeric', month: 'long', year: 'numeric' });

  function crear(etiqueta, clase, texto) {
    const nodo = document.createElement(etiqueta);
    if (clase) nodo.className = clase;
    if (texto !== undefined) nodo.textContent = texto;
    return nodo;
  }

  // ---------- HUD ----------
  /**
   * Pinta un medidor: ancho de la barra, número, estado «bajo» y valor accesible.
   * El ancho se fija por CSSOM (element.style), que la CSP permite (D-22).
   * @param {HTMLElement} elemento Elemento .medidor
   * @param {number} valor Valor entre 0 y 100
   */
  function pintarMedidor(elemento, valor) {
    const bajo = valor < REGLAS.UMBRAL_BAJO;
    const texto = porcentaje(valor);
    const barra = elemento.querySelector('[role="progressbar"]');
    elemento.classList.toggle('medidor--bajo', bajo);
    elemento.querySelector('.medidor__relleno').style.width = valor + '%';
    elemento.querySelector('.medidor__numero').textContent = texto;
    barra.setAttribute('aria-valuenow', String(valor));
    barra.setAttribute('aria-valuetext', bajo ? texto + ', nivel bajo' : texto);
  }

  function renderHud() {
    MEDIDORES.forEach((m) => pintarMedidor($('medidor-' + m.clave), ui.estado.medidores[m.clave]));
    const n = ui.estado.respaldos;
    document.querySelectorAll('.respaldo').forEach((icono, i) => icono.classList.toggle('respaldo--usado', i >= n));
    $('respaldos-texto').textContent = plantilla(TEXTOS.hud.respaldos, { n, total: REGLAS.RESPALDOS_INICIALES });
    renderProgreso();
  }

  function textoProgreso() {
    const { fase, nivel } = ui.estado;
    if (fase === FASES.INICIO) return TEXTOS.hud.inicio;
    if (fase === FASES.VICTORIA) return TEXTOS.hud.victoria;
    return plantilla(TEXTOS.hud.nivel, { n: nivel, total: NIVELES.length });
  }

  function renderProgreso() {
    const { fase, nivel } = ui.estado;
    $('progreso-texto').textContent = textoProgreso();
    document.querySelectorAll('.progreso__punto').forEach((punto, i) => {
      const numero = i + 1;
      const hecho = fase === FASES.VICTORIA || numero < nivel || (numero === nivel && fase === FASES.RESULTADO);
      punto.classList.toggle('progreso__punto--hecho', hecho);
      punto.classList.toggle('progreso__punto--actual', !hecho && numero === nivel);
    });
  }

  // ---------- Tarjeta ----------
  function renderTarjeta() {
    const visible = VISTA_POR_FASE[ui.estado.fase];
    document.querySelectorAll('.vista').forEach((vista) => { vista.hidden = vista.id !== visible; });
    if (visible === 'vista-nivel') renderNivel();
    if (visible === 'vista-victoria') renderCertificado();
  }

  function renderNivel() {
    const nivel = nivelActual();
    const { fase } = ui.estado;
    const historico = nivel.etiquetaHistorico && !ui.tormenta.enVivo;
    $('nivel-etiqueta').textContent = historico ? nivel.etiquetaHistorico : nivel.etiqueta;
    $('nivel-avatar').textContent = nivel.avatar;
    $('nivel-numero').textContent = plantilla(TEXTOS.tarjeta.prefijoNivel, { n: nivel.id });
    $('nivel-nombre').textContent = nivel.titulo;
    $('nivel-historia').textContent = nivel.historia;
    renderAvisos(nivel);
    $('resultado').hidden = fase !== FASES.RESULTADO;
    if (fase === FASES.RESULTADO) renderResultado(nivel);
  }

  function renderAvisos(nivel) {
    const { fase, claseX } = ui.estado;
    const avisoX = $('aviso-x');
    avisoX.hidden = !(nivel.penalizacionX && claseX);
    avisoX.textContent = plantilla(TEXTOS.penalizacionX, { n: REGLAS.PENALIZACION_X });
    const avisoAyuda = $('aviso-ayuda');
    avisoAyuda.hidden = !(ui.ayuda && fase === FASES.NIVEL);
    avisoAyuda.textContent = ui.ayuda ? textoAyuda(ui.ayuda) : '';
    $('aviso-respaldo').hidden = fase !== FASES.RESPALDO;
    if (fase === FASES.RESPALDO) renderRespaldo(nivel);
  }

  function textoAyuda(evento) {
    const recursos = Object.keys(evento.ajustes).map((k) => medidor(k).recurso).join(' y ');
    return plantilla(TEXTOS.ayuda, { recurso: recursos });
  }

  /** Mensaje del medidor que falló, por prioridad 💧 → 🛡️ → ⚡ (3.6), o el del requisito del Nivel 5. */
  function renderRespaldo(nivel) {
    const motivo = ui.estado.fallidos[0];
    if (motivo) {
      $('respaldo-mensaje').textContent = TEXTOS.respaldo[motivo];
      $('respaldo-pista').textContent = TEXTOS.respaldo.pista;
      return;
    }
    const opcion = nivel.opciones[ui.estado.ultimaOpcion];
    $('respaldo-mensaje').textContent = plantilla(TEXTOS.respaldo.condicion, { fallo: opcion.fallo });
    $('respaldo-pista').textContent = TEXTOS.respaldo.pistaCondicion;
  }

  function renderResultado(nivel) {
    $('resultado-texto').textContent = nivel.opciones[ui.estado.ultimaOpcion].resultado;
    $('dato-texto').textContent = nivel.dato.texto;
    const extra = $('dato-extra');
    extra.replaceChildren(...lineasExtra(nivel).map((t) => crear('p', 'dato__linea', t)));
    extra.hidden = extra.childElementCount === 0;
    pintarFuentes($('dato-fuentes'), fuentesDe(nivel));
  }

  /** Líneas extra del 💡: la de CSA y, en el Nivel 4, la llamarada real (sección 7). */
  function lineasExtra(nivel) {
    const lineas = [...(nivel.dato.extra || [])];
    if (!nivel.datoTormenta) return lineas;
    const t = ui.tormenta;
    lineas.push(plantilla(TEXTOS.llamarada.linea, { clase: t.clase, fecha: fechaUTC.format(new Date(t.fecha)) }));
    if (!t.enVivo) lineas.push(plantilla(TEXTOS.llamarada.serieX, { n: t.llamaradasX }));
    return lineas;
  }

  function fuentesDe(nivel) {
    if (!nivel.datoTormenta) return nivel.dato.fuentes;
    return [...nivel.dato.fuentes, { nombre: TEXTOS.llamarada.fuente, url: ui.tormenta.url }];
  }

  function pintarFuentes(contenedor, fuentes) {
    contenedor.replaceChildren((fuentes.length > 1 ? TEXTOS.tarjeta.fuentes : TEXTOS.tarjeta.fuente) + ' ');
    fuentes.forEach((f, i) => {
      if (i > 0) contenedor.append(' · ');
      contenedor.append(enlace(f.nombre, f.url));
    });
  }

  function enlace(texto, url) {
    const a = crear('a', '', texto);
    a.href = url;
    a.target = '_blank';
    a.rel = 'noopener noreferrer';
    a.append(crear('span', 'sr-only', TEXTOS.tarjeta.pestanaNueva));
    return a;
  }

  // ---------- Certificado (Victoria) ----------
  function renderCertificado() {
    const n = ui.estado.respaldos;
    $('certificado-fecha').textContent = fechaLocal.format(new Date());
    $('certificado-estrellas').textContent = '⭐'.repeat(n);
    $('certificado-respaldos').textContent = plantilla(TEXTOS.certificado.respaldos, { n, total: REGLAS.RESPALDOS_INICIALES });
    $('certificado-medidores').replaceChildren(...MEDIDORES.map(itemMedidor));
    $('certificado-datos').replaceChildren(...NIVELES.map(itemDato));
  }

  function itemMedidor(m) {
    const li = crear('li');
    li.append(crear('span', '', m.icono), ` ${m.nombre}: ${porcentaje(ui.estado.medidores[m.clave])}`);
    li.firstChild.setAttribute('aria-hidden', 'true');
    return li;
  }

  function itemDato(nivel) {
    const li = crear('li');
    const fuentes = crear('span', 'certificado__dato-fuentes');
    pintarFuentes(fuentes, fuentesDe(nivel));
    li.append(crear('span', 'certificado__dato-titulo', nivel.titulo), crear('span', '', nivel.dato.texto), fuentes);
    return li;
  }

  // ---------- Decisiones ----------
  function renderDecisiones() {
    const enNivel = ui.estado.fase === FASES.NIVEL;
    $('opciones').hidden = !enNivel;
    $('boton-principal').hidden = enNivel;
    if (enNivel) ['A', 'B'].forEach((id) => renderOpcion(id, nivelActual().opciones[id]));
    else $('boton-principal-texto').textContent = TEXTOS.botones[ui.estado.fase];
    actualizarBloqueo();
  }

  function renderOpcion(id, opcion) {
    const boton = $('opcion-' + id.toLowerCase());
    boton.classList.toggle('opcion--condicion', Boolean(opcion.requisito));
    boton.querySelector('.opcion__texto').textContent = opcion.texto;
    boton.querySelector('.opcion__efectos').replaceChildren(...lineasEfecto(opcion).map((t) => crear('span', 'efecto', t)));
    boton.querySelector('.opcion__descripcion').textContent = descripcion(opcion);
  }

  /** Lo que se ve en el botón: «+20 ⚡», «−35 💧» o el requisito del Nivel 5. */
  function lineasEfecto(opcion) {
    if (opcion.requisito) {
      const { medidor: clave, umbral } = opcion.requisito;
      return [plantilla(TEXTOS.opciones.requisito, { umbral, icono: medidor(clave).icono })];
    }
    return efectosOrdenados(opcion.efectos).map(([k, v]) => conSigno(v) + ' ' + medidor(k).icono);
  }

  /** Lo que oye el lector de pantalla: «Gana 30 de Protección y cuesta 30 de Batería.» */
  function descripcion(opcion) {
    if (opcion.requisito) {
      const { medidor: clave, umbral } = opcion.requisito;
      return '. ' + plantilla(TEXTOS.opciones.requisitoAccesible, { umbral, nombre: medidor(clave).nombre });
    }
    const partes = efectosOrdenados(opcion.efectos).map(([k, v]) =>
      plantilla(v > 0 ? TEXTOS.opciones.gana : TEXTOS.opciones.cuesta, { n: Math.abs(v), nombre: medidor(k).nombre }));
    return '. ' + mayuscula(partes.join(' y ')) + '.';
  }

  function actualizarBloqueo() {
    ['opcion-a', 'opcion-b', 'boton-principal'].forEach((id) => { $(id).disabled = ui.bloqueado; });
  }

  // ---------- Animaciones ----------
  function animar(eventos) {
    const maximos = eventos.filter((e) => e.tipo === 'medidorAlMaximo').map((e) => e.medidor);
    eventos.forEach((e) => {
      if (e.tipo === 'efectos') {
        efectosOrdenados(e.efectos).forEach(([k, v]) =>
          flotante(k, maximos.includes(k) ? TEXTOS.alMaximo : conSigno(v) + ' ' + medidor(k).icono, v > 0));
      }
      if (e.tipo === 'penalizacionX') flotante(e.medidor, conSigno(-e.cantidad) + ' ' + medidor(e.medidor).icono, false);
      if (e.tipo === 'ayudaActivada') Object.entries(e.ajustes).forEach(([k, v]) => flotante(k, conSigno(v) + ' ' + medidor(k).icono, true));
      if (e.tipo === 'respaldoActivado') temblar();
    });
  }

  function flotante(clave, texto, gana) {
    const nodo = $('medidor-' + clave).querySelector('.medidor__flotante');
    nodo.textContent = texto;
    nodo.className = 'medidor__flotante ' + (gana ? 'medidor__flotante--gana' : 'medidor__flotante--pierde');
    void nodo.offsetWidth; // reinicia la animación si se repite
    nodo.classList.add('medidor__flotante--activo');
  }

  function temblar() {
    const tarjeta = $('tarjeta');
    tarjeta.classList.remove('tarjeta--temblor');
    void tarjeta.offsetWidth;
    tarjeta.classList.add('tarjeta--temblor');
    setTimeout(() => tarjeta.classList.remove('tarjeta--temblor'), TIEMPOS.TEMBLOR_MS);
  }

  // ---------- Anuncios para el lector de pantalla ----------
  function decir(id, texto) {
    const region = $(id);
    region.textContent = '';
    setTimeout(() => { region.textContent = texto; }, TIEMPOS.ANUNCIO_MS);
  }

  function textoCambio(clave, delta, eventos) {
    const nombre = medidor(clave).nombre;
    if (eventos.some((e) => e.tipo === 'medidorAlMaximo' && e.medidor === clave)) return plantilla(TEXTOS.anuncios.alMaximo, { nombre });
    const forma = delta > 0 ? TEXTOS.anuncios.sube : TEXTOS.anuncios.baja;
    return plantilla(forma, { nombre, valor: porcentaje(ui.estado.medidores[clave]) });
  }

  function mensajesResultado(eventos) {
    const nivel = nivelActual();
    const efectos = buscar(eventos, 'efectos');
    const cambios = efectos ? efectosOrdenados(efectos.efectos).map(([k, v]) => textoCambio(k, v, eventos)) : [];
    return [nivel.opciones[ui.estado.ultimaOpcion].resultado, ...cambios, plantilla(TEXTOS.anuncios.dato, { texto: nivel.dato.texto })];
  }

  /** Polite para resultados y avisos; assertive solo para «Respaldo activado» (5.5). */
  function anunciar(eventos) {
    const { fase } = ui.estado;
    if (fase === FASES.RESPALDO) decir('anuncio-urgente', $('respaldo-mensaje').textContent);
    const mensajes = [];
    if (buscar(eventos, 'penalizacionX')) mensajes.push($('aviso-x').textContent);
    const ayuda = buscar(eventos, 'ayudaActivada');
    if (ayuda) mensajes.push(textoAyuda(ayuda));
    if (fase === FASES.RESULTADO) mensajes.push(...mensajesResultado(eventos));
    if (mensajes.length) decir('anuncio', mensajes.join(' '));
  }

  // ---------- Foco ----------
  function enfocar() {
    const { fase } = ui.estado;
    if (fase === FASES.RESULTADO || fase === FASES.RESPALDO) {
      const destino = $(fase === FASES.RESULTADO ? 'resultado' : 'aviso-respaldo');
      destino.scrollIntoView({ block: 'start', behavior: movimientoReducido() ? 'auto' : 'smooth' });
      $('boton-principal').focus({ preventScroll: true });
      return;
    }
    $('tarjeta').scrollTop = 0;
    $(TITULO_POR_FASE[fase]).focus({ preventScroll: true });
  }

  // ---------- Flujo ----------
  function render() {
    renderHud();
    renderTarjeta();
    renderDecisiones();
    const enNivel = [FASES.NIVEL, FASES.RESULTADO, FASES.RESPALDO].includes(ui.estado.fase);
    document.body.classList.toggle('alert-storm', enNivel && nivelActual().tema === 'alerta');
  }

  /** Aplica el resultado del motor: dibuja, anima, anuncia y bloquea los controles durante la transición. */
  function procesar(resultado) {
    if (resultado.estado === ui.estado) return; // el motor ignoró la acción
    ui.estado = resultado.estado;
    const ayuda = buscar(resultado.eventos, 'ayudaActivada');
    if (ayuda || ui.estado.fase !== FASES.NIVEL) ui.ayuda = ayuda || null;
    ui.bloqueado = true;
    render();
    animar(resultado.eventos);
    anunciar(resultado.eventos);
    setTimeout(() => {
      ui.bloqueado = false;
      actualizarBloqueo();
      enfocar();
    }, TIEMPOS.TRANSICION_MS);
  }

  function elegir(opcionId) {
    if (ui.bloqueado) return;
    procesar(MOTOR.aplicarDecision(ui.estado, opcionId));
  }

  function accionPrincipal() {
    if (ui.bloqueado) return;
    const contexto = { claseX: /^X/.test(ui.tormenta.clase) };
    const acciones = {
      INICIO: () => MOTOR.empezar(ui.estado, contexto),
      RESULTADO: () => MOTOR.continuar(ui.estado, contexto),
      RESPALDO: () => MOTOR.reintentar(ui.estado),
      VICTORIA: () => ({ estado: MOTOR.reiniciar(), eventos: [] }),
      MISION_ABORTADA: () => ({ estado: MOTOR.reiniciar(), eventos: [] }),
    };
    const accion = acciones[ui.estado.fase];
    if (accion) procesar(accion());
  }

  /** Atajos (5.5): A/B o 1/2 para elegir y Enter para continuar. */
  function alPulsarTecla(evento) {
    if (evento.altKey || evento.ctrlKey || evento.metaKey || evento.repeat) return;
    if (evento.target.closest('input, textarea, select, [contenteditable="true"]')) return;
    const tecla = evento.key.toLowerCase();
    const opcion = { a: 'A', 1: 'A', b: 'B', 2: 'B' }[tecla];
    if (opcion && ui.estado.fase === FASES.NIVEL) {
      evento.preventDefault();
      elegir(opcion);
    } else if (tecla === 'enter' && !evento.target.closest('button, a') && !$('boton-principal').hidden) {
      evento.preventDefault();
      accionPrincipal();
    }
  }

  function enlazar() {
    $('opcion-a').addEventListener('click', () => elegir('A'));
    $('opcion-b').addEventListener('click', () => elegir('B'));
    $('boton-principal').addEventListener('click', accionPrincipal);
    document.addEventListener('keydown', alPulsarTecla);
  }

  BL.ui = Object.freeze({ pintarMedidor });
  enlazar();
  render();
})(window.BaseLunar);
