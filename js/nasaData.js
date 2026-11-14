/*
 * Base Lunar · js/nasaData.js
 * Datos de NASA DONKI para el Nivel 4.
 * F0: respaldo histórico (copia literal). F3: consulta en vivo con timeout, validación y caché.
 */
'use strict';

window.BaseLunar = window.BaseLunar || {};

window.BaseLunar.nasaData = {
  /**
   * Copia literal de NASA DONKI (FLR), descargada el 14-11-2026 de
   * https://ccmc.gsfc.nasa.gov/DONKI/WS/get/FLR?startDate=2024-05-14&endDate=2024-05-15
   * Se guardan solo las 5 llamaradas de clase X del 14 y 15 de mayo de 2024, con los mismos
   * campos y valores que devuelve el servicio (solo cambia la sangría). Ver docs/FUENTES.md, F-12 y F-13.
   */
  LLAMARADAS_X_MAYO_2024: [
    {
      "flrID": "2024-05-14T02:03:00-FLR-001",
      "catalog": "M2M_CATALOG",
      "instruments": [
        {
          "displayName": "GOES-P: EXIS 1.0-8.0"
        }
      ],
      "beginTime": "2024-05-14T02:03Z",
      "peakTime": "2024-05-14T02:09Z",
      "endTime": "2024-05-14T02:19Z",
      "classType": "X1.7",
      "sourceLocation": "S19W89",
      "activeRegionNum": 13664,
      "note": "",
      "link": "https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/30832/-1",
      "linkedEvents": [
        {
          "activityID": "2024-05-14T02:24:00-CME-001"
        }
      ],
      "sentNotifications": [
        {
          "messageID": "20240514-AL-001",
          "messageIssueTime": "2024-05-14T02:10Z",
          "messageURL": "https://ccmc.gsfc.nasa.gov/DONKI/view/Alert/30833/1"
        },
        {
          "messageID": "20240514-AL-002",
          "messageIssueTime": "2024-05-14T02:26Z",
          "messageURL": "https://ccmc.gsfc.nasa.gov/DONKI/view/Alert/30834/1"
        }
      ],
      "submissionTime": "2024-05-14T12:22Z",
      "versionId": 3
    },
    {
      "flrID": "2024-05-14T12:40:00-FLR-001",
      "catalog": "M2M_CATALOG",
      "instruments": [
        {
          "displayName": "GOES-P: EXIS 1.0-8.0"
        }
      ],
      "beginTime": "2024-05-14T12:40Z",
      "peakTime": "2024-05-14T12:55Z",
      "endTime": "2024-05-14T13:05Z",
      "classType": "X1.2",
      "sourceLocation": "S19W90",
      "activeRegionNum": 13664,
      "note": "This flare occurs near/slightly beyond the western limb, but likely originates from former Active Region 13664.",
      "link": "https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/30837/-1",
      "linkedEvents": [
        {
          "activityID": "2024-05-14T13:12:00-CME-001"
        }
      ],
      "sentNotifications": [
        {
          "messageID": "20240514-AL-003",
          "messageIssueTime": "2024-05-14T12:54Z",
          "messageURL": "https://ccmc.gsfc.nasa.gov/DONKI/view/Alert/30838/1"
        },
        {
          "messageID": "20240514-AL-004",
          "messageIssueTime": "2024-05-14T13:04Z",
          "messageURL": "https://ccmc.gsfc.nasa.gov/DONKI/view/Alert/30840/1"
        }
      ],
      "submissionTime": "2024-05-14T13:15Z",
      "versionId": 3
    },
    {
      "flrID": "2024-05-14T16:46:00-FLR-001",
      "catalog": "M2M_CATALOG",
      "instruments": [
        {
          "displayName": "GOES-P: EXIS 1.0-8.0"
        }
      ],
      "beginTime": "2024-05-14T16:46Z",
      "peakTime": "2024-05-14T16:51Z",
      "endTime": "2024-05-14T17:02Z",
      "classType": "X8.7",
      "sourceLocation": "S19W90",
      "activeRegionNum": 13664,
      "note": "AR 3664 is likely ~S19W96 at this time, but W90 used as we cannot determine this yet.",
      "link": "https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/30849/-1",
      "linkedEvents": [
        {
          "activityID": "2024-05-14T17:00:00-CME-001"
        }
      ],
      "sentNotifications": [
        {
          "messageID": "20240514-AL-006",
          "messageIssueTime": "2024-05-14T16:52Z",
          "messageURL": "https://ccmc.gsfc.nasa.gov/DONKI/view/Alert/30850/1"
        },
        {
          "messageID": "20240514-AL-007",
          "messageIssueTime": "2024-05-14T17:00Z",
          "messageURL": "https://ccmc.gsfc.nasa.gov/DONKI/view/Alert/30851/1"
        }
      ],
      "submissionTime": "2024-05-14T17:13Z",
      "versionId": 3
    },
    {
      "flrID": "2024-05-15T08:18:00-FLR-001",
      "catalog": "M2M_CATALOG",
      "instruments": [
        {
          "displayName": "GOES-P: EXIS 1.0-8.0"
        }
      ],
      "beginTime": "2024-05-15T08:18Z",
      "peakTime": "2024-05-15T08:37Z",
      "endTime": "2024-05-15T08:52Z",
      "classType": "X3.4",
      "sourceLocation": "S20W90",
      "activeRegionNum": 13664,
      "note": "",
      "link": "https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/30879/-1",
      "linkedEvents": [
        {
          "activityID": "2024-05-15T08:48:00-CME-001"
        }
      ],
      "sentNotifications": [
        {
          "messageID": "20240515-AL-002",
          "messageIssueTime": "2024-05-15T08:36Z",
          "messageURL": "https://ccmc.gsfc.nasa.gov/DONKI/view/Alert/30880/1"
        },
        {
          "messageID": "20240515-AL-003",
          "messageIssueTime": "2024-05-15T12:29Z",
          "messageURL": "https://ccmc.gsfc.nasa.gov/DONKI/view/Alert/30882/1"
        }
      ],
      "submissionTime": "2024-05-15T12:21Z",
      "versionId": 2
    },
    {
      "flrID": "2024-05-15T14:20:00-FLR-001",
      "catalog": "M2M_CATALOG",
      "instruments": [
        {
          "displayName": "GOES-P: EXIS 1.0-8.0"
        }
      ],
      "beginTime": "2024-05-15T14:20Z",
      "peakTime": "2024-05-15T14:38Z",
      "endTime": "2024-05-15T14:51Z",
      "classType": "X2.9",
      "sourceLocation": "S12E90",
      "activeRegionNum": 13685,
      "note": "Active Region 3685 was numbered on 2024-05-16. Could be the return of old AR 3654, which had several M-class flares (including an M9.5) on its previous rotation.",
      "link": "https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/30900/-1",
      "linkedEvents": [
        {
          "activityID": "2024-05-15T14:53:00-CME-001"
        }
      ],
      "sentNotifications": [
        {
          "messageID": "20240515-AL-006",
          "messageIssueTime": "2024-05-15T14:30Z",
          "messageURL": "https://ccmc.gsfc.nasa.gov/DONKI/view/Alert/30901/1"
        },
        {
          "messageID": "20240515-AL-008",
          "messageIssueTime": "2024-05-15T15:15Z",
          "messageURL": "https://ccmc.gsfc.nasa.gov/DONKI/view/Alert/30903/1"
        }
      ],
      "submissionTime": "2024-05-16T14:25Z",
      "versionId": 4
    }
  ],
};
