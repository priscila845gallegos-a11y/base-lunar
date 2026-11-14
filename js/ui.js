/*
 * Base Lunar · js/ui.js
 * Vista: el único archivo que toca el DOM.
 * F1: pinta los medidores de ejemplo. F2: render completo, eventos, accesibilidad y voz.
 */
'use strict';

window.BaseLunar = window.BaseLunar || {};

(function (BL) {
  const REGLAS = BL.config.REGLAS;

  /**
   * Pinta un medidor: ancho de la barra, número, estado «bajo» y valor accesible.
   * El ancho se fija por CSSOM (element.style), que la CSP permite; los atributos style no.
   * @param {HTMLElement} medidor Elemento .medidor
   * @param {number} valor Valor entre 0 y 100
   */
  function pintarMedidor(medidor, valor) {
    const bajo = valor < REGLAS.UMBRAL_BAJO;
    const texto = valor + ' %';
    const barra = medidor.querySelector('[role="progressbar"]');
    medidor.classList.toggle('medidor--bajo', bajo);
    medidor.querySelector('.medidor__relleno').style.width = valor + '%';
    medidor.querySelector('.medidor__numero').textContent = texto;
    barra.setAttribute('aria-valuenow', String(valor));
    barra.setAttribute('aria-valuetext', bajo ? texto + ', nivel bajo' : texto);
  }

  /** F1: pinta los valores de ejemplo escritos en el HTML (aria-valuenow). */
  function pintarEjemplo() {
    document.querySelectorAll('.medidor').forEach((medidor) => {
      const barra = medidor.querySelector('[role="progressbar"]');
      pintarMedidor(medidor, Number(barra.getAttribute('aria-valuenow')));
    });
  }

  BL.ui = Object.freeze({ pintarMedidor });
  pintarEjemplo();
})(window.BaseLunar);
