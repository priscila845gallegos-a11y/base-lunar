/*
 * Base Lunar · js/config.js
 * Configuración global: solo datos, sin lógica. Se carga primero (ver README, «Carga de scripts»).
 */
'use strict';

window.BaseLunar = window.BaseLunar || {};

window.BaseLunar.config = Object.freeze({
  /** Clave personal de api.nasa.gov (DECISIONES.md, D-10). Es visible en el navegador: no reutilizarla en otros servicios. */
  NASA_CLAVE: 'gCcCt7wbkbQJNb6PLrqFPNttPwgwjRFGdWkjNDQj',
  /** Último recurso. Documentación: 30/h y 50/día por IP; el 14-11-2026 respondió 429 a la primera llamada. */
  NASA_CLAVE_RESERVA: 'DEMO_KEY',
  /** Endpoint de llamaradas solares (FLR) de NASA DONKI. */
  NASA_URL_FLR: 'https://api.nasa.gov/DONKI/FLR',
  /** Rango de la consulta: los últimos N días. */
  NASA_DIAS_CONSULTA: 30,
  /** Tiempo máximo de espera de la consulta (AbortController). */
  NASA_TIMEOUT_MS: 3000,
  /** Validez de un resultado guardado en localStorage. */
  NASA_CACHE_MS: 6 * 60 * 60 * 1000,
  /** Clave de localStorage para la caché de DONKI. */
  NASA_CACHE_CLAVE: 'baseLunar.donki.v1',
  /** Respaldo histórico: llamarada X8.7 del 14-05-2024 (sección 7 del Prompt 0). */
  FLR_RESPALDO_ID: '2024-05-14T16:46:00-FLR-001',
  /** Nombres de los parámetros de demostración en la URL. */
  PARAMETROS_DEMO: Object.freeze({ NIVEL: 'nivel', OFFLINE: 'offline', DEBUG: 'debug' }),
});
