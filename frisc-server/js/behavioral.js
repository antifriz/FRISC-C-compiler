var MODUS_PLAN = 0, MODUS_TABELLE = 1;
var THEMA_LINIEN = 0, THEMA_SOFTWARE = 1, THEMA_DATEN = 2, THEMA_FPA = 3, THEMA_GIS = 4, THEMA_DFI = 5, THEMA_SVG = 6;
var HISTORIE_LIVE = 0, HISTORIE_STOP = 1, HISTORIE_BACK = 2, HISTORIE_FORW = 3, HISTORIE_STOP_SLIDER = 4;
var sAjaxRequest = false;
var sNetzplanDatenAnzeigen = new Array();
var sGrenzwertSoftwareSehrGut;
var sGrenzwertSoftwareGut;
var sGrenzwertDatenSehrGut;
var sGrenzwertDatenGut;
var sPromise;
var sMapDfiNuTabelle = new Array();
var sMapPhoneNumber = new Array();
var sMitGisView;
var sMitSvgView;
var sVersionStammdataIst = new Array(0, 0, 0);
var sVersionStammdataSoll = new Array(0, 0, 0);
var FrequenzFahrzeugStatusUpdate_Sekunden = 5;
var FrequenzDfiStatusUpdate_Minuten = 3;
var sTimeOutDataRequest = new Array(10, 10, 90);
var sTimestampLastRequest = new Array(0, 0, 0);
var sColorSelected = "#E00000";
var DEBUG = 0;
var _S, _E, _K;
var sHistorySlider;
var sUpdateProzess = 0;
function init() {
    $.ajaxSetup({
        timeout : (30 * 1000)
    });
    _S = {
        Modus : MODUS_PLAN,
        ThemaPlan : THEMA_LINIEN,
        ThemaPlanParamter : 1,
        ALinie : 0,
        AAnzeige : 0,
        ZFahrzeuge : "",
        ZAnzeigen : "",
        B : 0,
        HModus : HISTORIE_LIVE
    };
    _E = {
        RegioView : findenObjekt("ID_Regioview"),
        GisView : findenObjekt("ID_Map"),
        SvgView : findenObjekt("ID_Svg"),
        SchemaView : findenObjekt("ID_SchemaView"),
        LabelStatus : eval(document.getElementById("ID_online").firstChild),
        MenuWerkzeug : findenObjekt("KarteThemaAuswahl"),
        MenuWerkzeugLinie : findenObjekt("ID_karteThemaLinie"),
        MenuWerkzeugFpa : findenObjekt("ID_karteThemaFPA"),
        MenuWerkzeugGis : findenObjekt("ID_karteThemaGIS"),
        MenuWerkzeugSvg : findenObjekt("ID_karteThemaSVG"),
        MenuWerkzeugDfi0 : findenObjekt("ID_karteThemaDFI0"),
        MenuWerkzeugDfi1 : findenObjekt("ID_karteThemaDFI1"),
        MenuWerkzeugDfi6 : findenObjekt("ID_karteThemaDFI6"),
        MenuWerkzeugDfi24 : findenObjekt("ID_karteThemaDFI24"),
        MenuWerkzeugDfi48 : findenObjekt("ID_karteThemaDFI48"),
        MenuWerkzeugSoftware : findenObjekt("ID_karteThemaSoftware"),
        MenuWerkzeugDaten : findenObjekt("ID_karteThemaDaten"),
        BereichWerkzeugLeiste : findenObjekt("ID_WerkzeugLeiste"),
        ButtonHistorieBack : findenObjekt("ID_Historie_BACK"),
        ButtonHistorieForw : findenObjekt("ID_Historie_FORW"),
        ButtonHistorieLive : findenObjekt("ID_Historie_LIVE"),
        ButtonHistorieStop : findenObjekt("ID_Historie_STOP"),
        ButtonSelectLine : findenObjekt("ID_SelectLine")
    };
    _K = {
        H : new Array(),
        U : new Array()
    };
    sHistorySlider = {
        position_minimum : 200,
        position_original : parseInt(_E.BereichWerkzeugLeiste.style.left),
        position_movement : 0,
        HIndex : 0,
        faktor : 1440 / (parseInt(_E.BereichWerkzeugLeiste.style.left) - 200),
        reset : function() {
            this.setHIndex(0);
            _E.LabelStatus.parentNode.style.visibility = "visible";
            _E.ButtonHistorieStop.title = "Stop";
        },
        start : function(e) {
            var tx = parseInt(_E.BereichWerkzeugLeiste.style.left);
            if (document.all) {
                this.position_movement = event.clientX + document.body.scrollLeft - tx;
            } else {
                this.position_movement = e.pageX - tx;
            }
            _E.LabelStatus.parentNode.style.visibility = "hidden";
        },
        move : function(e) {
            var x;
            if (document.all) {
                x = event.clientX + document.body.scrollLeft;
            } else {
                x = e.pageX;
            }
            var Position = x - this.position_movement;
            this.setHIndex(Math.round((this.position_original - Position) * this.faktor));
            var Timestamp = new Date(new Date().getTime() - this.HIndex * 29.5 * 1000);
            var Minuten = Timestamp.getMinutes();
            _E.ButtonHistorieStop.title = Timestamp.getHours() + ":" + (Minuten < 10 ? "0" : "") + Minuten;
        },
        stepback : function() {
            this.step(1);
        },
        stepforward : function() {
            this.step(-1);
        },
        step : function(step) {
            this.setHIndex(this.HIndex + step);
            _E.LabelStatus.parentNode.style.visibility = "hidden";
            _E.ButtonHistorieStop.title = "Stop";
        },
        setHIndex : function(value) {
            this.HIndex = Math.max(0, value);
            this.setPosition(this.position_original - (this.HIndex / this.faktor));
        },
        setPosition : function(position) {
            _E.BereichWerkzeugLeiste.style.left = Math.max(this.position_minimum, Math.min(position, this.position_original));
        }
    };
    uebersetzen(_E.MenuWerkzeug);
    if (_E.MenuWerkzeugLinie) {
        _E.MenuWerkzeugLinie.style.color = sColorSelected;
        _E.MenuWerkzeugLinie.focus();
    } else {
        _S.ThemaPlan = THEMA_GIS;
        sMitGisView = GV_init(_E.GisView, document.getElementById("ID_time").firstChild, Configuration);
        if (!sMitGisView) {
            anzeigenFehler(getI18nMessage("GIS view not available. Please check the Internet configuration for your computer."));
        } else {
            _E.SchemaView.style.visibility = "hidden";
            GV_zeigen();
        }
    }
    sAjaxRequest = new myAjax();
    if (!sAjaxRequest) {
        anzeigenFehler(getI18nMessage("The application runs only on browsers with AJAX support."));
        return 0;
    }
    var vContextText = new Array(getI18nMessage("Fleet Status"), getI18nMessage("Dpi Status"), getI18nMessage("Timetable"), getI18nMessage("Vehicles"), getI18nMessage("Daily Report"), getI18nMessage("GPS Report"), getI18nMessage("Vehicle Groups"));
    var vContextAktion = new Array("_aendernModus()", "_showDfiData()", "hideContextMenu();showDetailView_Fahrplan()", "hideContextMenu();showDetailView_Fahrzeugdetails()", "hideContextMenu();showDetailView_UmlaufReport()", "hideContextMenu();showDetailView_GpsLog()", "hideContextMenu();showDetailView_Fahrzeuggruppen()");
    var vContextRechte = new Array(1, Configuration.Dfi, 1, 1, 1, 1, 1);
    if (Configuration.Vdv) {
        vContextText.push(getI18nMessage("VDV / CUS"));
        vContextAktion.push("hideContextMenu();showDetailView_VdvPartner()");
        vContextRechte.push(1);
    }
    if (Configuration.Mandant) {
        vContextText.push(getI18nMessage("Change Client"));
        vContextAktion.push("hideContextMenu();showMandantenListe()");
        vContextRechte.push(1);
    }
    registerContextMenu(window, "ID_Regioview", definiereContextMenu(getI18nMessage("WebRBL"), vContextText, vContextAktion, vContextRechte));
    document.oncontextmenu = handleRightMouseClick;
    document.onclick = hideContextMenu;
    Netzplan_init(top.map, document.getElementById("ID_time").firstChild);
    top.map.document.oncontextmenu = handleRightMouseClick;
    top.map.document.onclick = hideContextMenu;
    initTabelle(TABELLE_BETRIEBSLAGE, [[1, "asc"], [0, "asc"]], [SPALTE_UMLAUF_MITLINK_TABELLE_FAHRTEN, SPALTE_LINIE, SPALTE_FAHRT, SPALTE_ZIEL_MITLINK_TABELLE_HALTESTELLE, SPALTE_FAHRZEUG_KURZ, SPALTE_POSITION, SPALTE_DELAY, SPALTE_PROBLEME_KURZ]);
    var stationTblColumns = [];
    stationTblColumns.push(SPALTE_HST_NR);
    stationTblColumns.push(SPALTE_HST_NAME);
    if (Configuration.Mandant) {
        stationTblColumns.push(SPALTE_LINK_VHST);
    }
    stationTblColumns.push(SPALTE_LINK_LINIEN);
    stationTblColumns.push(SPALTE_MASTEN);
    stationTblColumns.push(SPALTE_LON);
    stationTblColumns.push(SPALTE_LAT);
    if (Configuration.Dfi) {
        stationTblColumns.push(SPALTE_DPIS);
    }
    if (Configuration.Vdv) {
        stationTblColumns.push(SPALTE_VDV_DIS_KURZ);
    }
    initTabelle(TABELLE_HALTESTELLEN, [[0, "asc"]], stationTblColumns);
    if (Configuration.Dfi) {
        initTabelle(TABELLE_ANZEIGEN, [[0, "asc"]], [SPALTE_DPI_NR, SPALTE_HAST, SPALTE_HAST_NAME, SPALTE_HAST_STATUS, SPALTE_Access01, SPALTE_Access06, SPALTE_Access24, SPALTE_Access48, SPALTE_HAST_DATA_EXPIRATION]);
    }
    updateStatusData();
    if (Configuration.Dfi) {
        updateDfiStatusData();
    }
    _E.ButtonHistorieBack.onclick = historie_back;
    _E.ButtonHistorieForw.onclick = historie_forw;
    _E.ButtonHistorieLive.onclick = historie_live;
    _E.ButtonHistorieStop.onclick = historie_stop;
    _E.ButtonSelectLine.onclick = _selectLineOrService;
    _E.ButtonHistorieStop.onmousedown = historie_startslider;
    _E.ButtonHistorieStop.onmousemove = historie_runslider;
    _E.ButtonHistorieStop.onmouseup = historie_stopslider;
}

function uebersetzen(aElement) {
    if (aElement.data) {
        aElement.data = getI18nMessage(aElement.data);
    }
    var vChildNodes = aElement.childNodes;
    for (var i = 0; i < vChildNodes.length; i++) {
        uebersetzen(vChildNodes[i]);
    }
}

function close() {
    if (sMitGisView) {
        GV_close();
    }
}

function historie_live() {
    _E.ButtonHistorieLive.src = "rsrc/media-playback-start_.png";
    _E.ButtonHistorieStop.src = "rsrc/media-playback-stop.png";
    _E.ButtonHistorieBack.src = "rsrc/media-seek-backward.png";
    _E.ButtonHistorieForw.src = "rsrc/media-seek-forward.png";
    _S.HModus = HISTORIE_LIVE;
    sHistorySlider.reset();
    updateStatusData();
}

function historie_stop() {
    _E.ButtonHistorieLive.src = "rsrc/media-playback-start.png";
    _E.ButtonHistorieStop.src = "rsrc/media-playback-stop_.png";
    _E.ButtonHistorieBack.src = "rsrc/media-seek-backward.png";
    _E.ButtonHistorieForw.src = "rsrc/media-seek-forward.png";
    _S.HModus = HISTORIE_STOP;
}

function historie_back() {
    _E.ButtonHistorieLive.src = "rsrc/media-playback-start.png";
    _E.ButtonHistorieStop.src = "rsrc/media-playback-stop.png";
    _E.ButtonHistorieBack.src = "rsrc/media-seek-backward_.png";
    _E.ButtonHistorieForw.src = "rsrc/media-seek-forward.png";
    _S.HModus = HISTORIE_BACK;
    sHistorySlider.stepback();
    updateStatusData();
}

function historie_forw() {
    _E.ButtonHistorieLive.src = "rsrc/media-playback-start.png";
    _E.ButtonHistorieStop.src = "rsrc/media-playback-stop.png";
    _E.ButtonHistorieBack.src = "rsrc/media-seek-backward.png";
    _E.ButtonHistorieForw.src = "rsrc/media-seek-forward_.png";
    if (sHistorySlider.HIndex > 0) {
        _S.HModus = HISTORIE_FORW;
        sHistorySlider.stepforward();
        updateStatusData();
    } else {
        historie_live();
    }
}

function historie_startslider(e) {
    if (_S.HModus == HISTORIE_STOP) {
        _S.HModus = HISTORIE_STOP_SLIDER;
        sHistorySlider.start(e);
        sDatenTabelle[TABELLE_BETRIEBSLAGE] = new Array();
        _S.B = [];
        if (_S.Modus == MODUS_TABELLE && getThemaTabelle() == TABELLE_BETRIEBSLAGE) {
            schreibeTabelle();
        } else {
            if (_S.ThemaPlan != THEMA_GIS) {
                Netzplan_update();
            } else {
                if (sMitGisView) {
                    GV_update();
                }
            }
        }
    }
    return false;
}

function historie_runslider(e) {
    if (_S.HModus == HISTORIE_STOP_SLIDER) {
        sHistorySlider.move(e);
    }
    return false;
}

function historie_stopslider() {
    if (_S.HModus == HISTORIE_STOP_SLIDER) {
        updateStatusData();
        historie_stop();
    }
    return false;
}

function handleRightMouseClick(inEvent) {
    var event = (inEvent) ? inEvent : (top.map.event ? top.map.event : top.event);
    event.cancelBubble = true;
    if (_S.Modus == MODUS_PLAN) {
        showContextMenu(event);
    } else {
        _aendernModus(event);
    }
    return false;
}

function sendDataRequest(aDaten, aParameter) {
    var request = $.get("page?data=" + aDaten + ( aParameter ? "&param=" + aParameter : "") + (Configuration.Mandant ? "&Mandant=" + Configuration.Mandant : "") + "&" + Math.random(), _handleDataResponse);
    _E.LabelStatus.data = aDaten + ( aParameter ? " " + aParameter : "") + " requested..";
    return request.promise();
}

function _handleDataResponse(data) {
    var xmldoc = data;
    if (DEBUG) {
        alert(sAjaxRequest.getResponseText());
    }
    var vContent = xmldoc.getElementsByTagName("content")[0].firstChild.data;
    _E.LabelStatus.data = vContent + " loading..";
    try {
        if (vContent == "STATUS") {
            sTimestampLastRequest[0] = 0;
            if (_S.HModus != HISTORIE_STOP_SLIDER) {
                handleFahrzeugStatusData(xmldoc);
            }
        } else {
            if (vContent == "DFISTATUS") {
                sTimestampLastRequest[1] = 0;
                handleDfiStatusData(xmldoc);
            } else {
                if (vContent == "HALTESTELLE") {
                    sTimestampLastRequest[2] = 0;
                    leseStammdatenHaltestellen(xmldoc);
                } else {
                    if (vContent == "FAHRZEUG") {
                        sTimestampLastRequest[2] = 0;
                        leseStammdatenFahrzeuge(xmldoc);
                    } else {
                        if (vContent == "LINIEN") {
                            sTimestampLastRequest[2] = 0;
                            leseStammdatenLinien(xmldoc);
                        } else {
                            anzeigenFehler("error while reading data: unknown data type '" + vContent + "'");
                        }
                    }
                }
            }
        }
        _E.LabelStatus.data = vContent + " loaded";
    } catch(e) {
        _E.LabelStatus.data = "Error loading " + vContent + ": " + e;
    }
}

function updateStatusData(aUpdateProzess) {
    if (!aUpdateProzess) {
        sUpdateProzess++;
        aUpdateProzess = sUpdateProzess;
    }
    if (aUpdateProzess < sUpdateProzess) {
        return;
    }
    var vTime = new Date().getTime();
    if ((!sTimestampLastRequest[1] || sTimestampLastRequest[1] < (vTime - sTimeOutDataRequest[1] * 1000)) && (!sTimestampLastRequest[2] || sTimestampLastRequest[2] < (vTime - sTimeOutDataRequest[2] * 1000))) {
        var vUpdateFrequenz = FrequenzFahrzeugStatusUpdate_Sekunden * 1000;
        try {
            sTimestampLastRequest[0] = new Date().getTime();
            if (_S.HModus == HISTORIE_BACK || _S.HModus == HISTORIE_FORW) {
                vUpdateFrequenz = 2000;
            }
            if (_S.HModus != HISTORIE_STOP) {
                sendDataRequest("status", sHistorySlider.HIndex).always(function() {
                    window.setTimeout("updateStatusData(" + aUpdateProzess + ")", vUpdateFrequenz);
                });
            }
        } catch(e) {
            alert(e);
            _E.LabelStatus.data = "error: " + e;
            window.setTimeout("updateStatusData(" + aUpdateProzess + ")", vUpdateFrequenz);
        }
    } else {
        window.setTimeout("updateStatusData(" + aUpdateProzess + ")", 1000);
    }
}

function updateDfiStatusData() {
    var vTime = new Date().getTime();
    if ((sDatenTabelle[TABELLE_HALTESTELLEN].length == 0) || (sTimestampLastRequest[2] && sTimestampLastRequest[2] > (vTime - sTimeOutDataRequest[2] * 1000))) {
        window.setTimeout("updateDfiStatusData()", 1000);
    } else {
        try {
            sTimestampLastRequest[1] = new Date().getTime();
            sendDataRequest("dfistatus");
        } catch(e) {
            _E.LabelStatus.data = "error: " + e;
        }
        window.setTimeout("updateDfiStatusData()", FrequenzDfiStatusUpdate_Minuten * 60 * 1000);
    }
}

function handleDfiStatusData(xmldoc) {
    sNetzplanDatenAnzeigen = new Array();
    for ( i = 0; i < sDatenTabelle[TABELLE_ANZEIGEN].length; i++) {
        for ( j = 3; j < 9; j++) {
            sDatenTabelle[TABELLE_ANZEIGEN][i][j] = "";
        }
    }
    var vAnzeigen = xmldoc.getElementsByTagName("anz");
    var vCntOhneIndex = 0;
    for ( i = 0; i < vAnzeigen.length; i++) {
        var vNummer = vAnzeigen[i].getAttribute("n");
        var vStatus = vAnzeigen[i].getAttribute("s");
        var vDataExpiration = vAnzeigen[i].getAttribute("v");
        var vErreichbarkeit0 = vAnzeigen[i].getAttribute("e1");
        var vErreichbarkeit1 = vAnzeigen[i].getAttribute("e6");
        var vErreichbarkeit2 = vAnzeigen[i].getAttribute("e24");
        var vErreichbarkeit3 = vAnzeigen[i].getAttribute("e48");
        var vIndex = sMapDfiNuTabelle[vNummer];
        if (vIndex) {
            if (!(vErreichbarkeit0 > 0 || vErreichbarkeit1 > 0 || vErreichbarkeit2 > 1 || vErreichbarkeit3 > 2)) {
                vErreichbarkeit0 = -1;
                vErreichbarkeit1 = -1;
                vErreichbarkeit2 = -1;
                vErreichbarkeit3 = -1;
                vStatus = -1;
            }
            sNetzplanDatenAnzeigen.push(new Array(vNummer, sDatenTabelle[TABELLE_ANZEIGEN][vIndex-1][1], sDatenTabelle[TABELLE_ANZEIGEN][vIndex-1][2], sDatenTabelle[TABELLE_ANZEIGEN][vIndex-1][9], sDatenTabelle[TABELLE_ANZEIGEN][vIndex-1][10], vStatus, vErreichbarkeit0, vErreichbarkeit1, vErreichbarkeit2, vErreichbarkeit3));
        } else {
            vCntOhneIndex++;
        }
        if (vErreichbarkeit0 == "-1.0") {
            vErreichbarkeit0 = "-";
        }
        if (vErreichbarkeit1 == "-1.0") {
            vErreichbarkeit1 = "-";
        }
        if (vErreichbarkeit2 == "-1.0") {
            vErreichbarkeit2 = "-";
        }
        if (vErreichbarkeit3 == "-1.0") {
            vErreichbarkeit3 = "-";
        }
        if (vStatus == "-1.0") {
            vStatus = "-";
        }
        if (vIndex) {
            sDatenTabelle[TABELLE_ANZEIGEN][vIndex-1][3] = vStatus;
            sDatenTabelle[TABELLE_ANZEIGEN][vIndex-1][4] = vErreichbarkeit0;
            sDatenTabelle[TABELLE_ANZEIGEN][vIndex-1][5] = vErreichbarkeit1;
            sDatenTabelle[TABELLE_ANZEIGEN][vIndex-1][6] = vErreichbarkeit2;
            sDatenTabelle[TABELLE_ANZEIGEN][vIndex-1][7] = vErreichbarkeit3;
            sDatenTabelle[TABELLE_ANZEIGEN][vIndex-1][8] = vDataExpiration;
        }
    }
    _S.ZAnzeigen = xmldoc.getElementsByTagName("time")[0].firstChild.data;
    changeZeitstempelTabelle(TABELLE_ANZEIGEN, _S.ZAnzeigen);
    if (_S.Modus == MODUS_TABELLE && getThemaTabelle() == TABELLE_ANZEIGEN) {
        schreibeTabelle();
    } else {
        if (_S.Modus == MODUS_PLAN && _S.ThemaPlan == THEMA_DFI) {
            Netzplan_update();
        }
    }
}

function handleFahrzeugStatusData(xmldoc) {
    TimestampLastFahrzeugStatusUpdate = new Date().getTime();
    if (_S.HModus == HISTORIE_BACK) {
        sHistorySlider.stepback();
    } else {
        if (_S.HModus == HISTORIE_FORW) {
            sHistorySlider.stepforward();
        }
    }
    if (sHistorySlider.HIndex < 0) {
        historie_live();
        sHistorySlider.reset();
    }
    sGrenzwertSoftwareSehrGut = xmldoc.getElementsByTagName("swb")[0].firstChild.data;
    sGrenzwertSoftwareGut = xmldoc.getElementsByTagName("swg")[0].firstChild.data;
    sGrenzwertDatenSehrGut = xmldoc.getElementsByTagName("dab")[0].firstChild.data;
    sGrenzwertDatenGut = xmldoc.getElementsByTagName("dag")[0].firstChild.data;
    var vStammdataVersion = xmldoc.getElementsByTagName("base")[0];
    sVersionStammdataSoll[0] = vStammdataVersion.getAttribute("H");
    sVersionStammdataSoll[1] = vStammdataVersion.getAttribute("F");
    sVersionStammdataSoll[2] = sVersionStammdataSoll[0];
    checkStammdaten();
    if (sPromise && !(sPromise.state() === "resolved")) {
        return;
    }
    var vDataAlt = _S.B;
    sDatenTabelle[TABELLE_BETRIEBSLAGE] = new Array();
    delete (_S.B);
    _S.B = [];
    var fahrten = xmldoc.getElementsByTagName("frt");
    for ( i = 0; i < fahrten.length; i++) {
        var lid = fahrten[i].getAttribute("u");
        _S.B[lid] = {
            Li : 0,
            Fid : 0,
            Real : 0,
            Ziel : 0,
            Soft : 0,
            Data : 0,
            PosStr : "",
            PosLon : 0,
            PosLat : 0,
            Fpa : 0,
            FpaStr : "",
            PosTop : 0,
            PosLeft : 0,
            PosZ : 0
        };
        _S.B[lid].Li = fahrten[i].getAttribute("l");                        //line
        _S.B[lid].Fid = fahrten[i].getAttribute("f");                       //ID?
        _S.B[lid].Real = fahrten[i].getAttribute("r");                      //real?
        _S.B[lid].Ziel = aufloesenHstNummer(fahrten[i].getAttribute("d"));  //target, destination

        _S.B[lid].Soft = fahrten[i].getAttribute("s");                      //soft?
        _S.B[lid].Data = fahrten[i].getAttribute("t");                      //data?
        _S.B[lid].Fpa = -fahrten[i].getAttribute("a");                      //Fpa?
        _S.B[lid].PosTop = fahrten[i].getAttribute("h");                    //PosTop
        _S.B[lid].PosLeft = fahrten[i].getAttribute("b");                   //PosLeft
        _S.B[lid].PosZ = fahrten[i].getAttribute("z");                      //PosZ
        _S.B[lid].NextF = fahrten[i].getAttribute("nf");                    //NextF
        _S.B[lid].RouteV = fahrten[i].getAttribute("rv");                   //RouteV
        _S.B[lid].Lon = fahrten[i].getAttribute("rx");                      //Lon
        _S.B[lid].Lat = fahrten[i].getAttribute("ry");                      //Lat
        var vVon = _S.B[lid].From = fahrten[i].getAttribute("v");           //From
        var vNach = _S.B[lid].To = fahrten[i].getAttribute("n");            //To
        var vProzent = _S.B[lid].Perc = fahrten[i].getAttribute("p");       //Perc
        _S.B[lid].PosStr = aufloesenHstNummer(vVon);
        if (vVon != vNach && !_S.B[lid].RouteV) {
            _S.B[lid].PosStr += " -> " + aufloesenHstNummer(vNach);
        } else {
            if (vVon != vNach && _S.B[lid].RouteV) {
                _S.B[lid].PosStr += " -> ***";
            } else {
                if (_S.B[lid].RouteV) {
                    _S.B[lid].PosStr += " ***";
                }
            }
        }
        if ((!_S.B[lid].Lon) && _D.H[vVon] && _D.H[vNach]) {
            var vVonLon = _D.H[vVon].Lon;
            var vNachLon = _D.H[vNach].Lon;
            if (vVonLon && vNachLon) {
                var vVonLat = _D.H[vVon].Lat;
                var vNachLat = _D.H[vNach].Lat;
                _S.B[lid].Lon = vVonLon * 1 + (vNachLon - vVonLon) * vProzent / 100;
                _S.B[lid].Lat = vVonLat * 1 + (vNachLat - vVonLat) * vProzent / 100;
            }
        }
        var prob = fahrten[i].getAttribute("k");
        if (_S.B[lid].Real) {
            if (_S.B[lid].Fpa < 0) {
                _S.B[lid].FpaStr = "-";
            }
            var value = Math.abs(_S.B[lid].Fpa);
            var fpaSek = value % 60;
            _S.B[lid].FpaStr += (value - fpaSek) / 60 + "'";
            if (fpaSek < 10) {
                _S.B[lid].FpaStr += "0";
            }
            _S.B[lid].FpaStr += fpaSek;
        }
        sDatenTabelle[TABELLE_BETRIEBSLAGE].push(new Array(lid, _S.B[lid].Li + "|" + _D.getLineLabel(_S.B[lid].Li), _S.B[lid].Fid, _S.B[lid].Ziel, _S.B[lid].Real ? _S.B[lid].Real : "", _S.B[lid].PosStr, _S.B[lid].FpaStr, prob ? prob : ""));
        if (!_K.U[lid] || !vDataAlt[lid] || vDataAlt[lid].Fid != _S.B[lid].Fid || vDataAlt[lid].Li != _S.B[lid].Li || vDataAlt[lid].Real != _S.B[lid].Real) {
            _K.U[lid] = definiereContextMenu(getI18nMessage("Service") + " " + lid, new Array(getI18nMessage("List of trips"), getI18nMessage("Current trip"), getI18nMessage("Following trip"), getI18nMessage("Call driver"), getI18nMessage("Message to driver"), getI18nMessage("Delay history"), getI18nMessage("Critical event list"), getI18nMessage("Select line") + " " + _D.getLineLabel(_S.B[lid].Li)), new Array("showDetailView_Umlauf(" + lid + ")", "showDetailView_Fahrt('" + lid + " " + _S.B[lid].Li + " " + _S.B[lid].Fid + "')", "showDetailView_Fahrt('" + lid + " " + _S.B[lid].NextF + "')", "anrufenFahrzeug('" + _S.B[lid].Real + "')", "RegioviewFunction_sendMessage('" + _S.B[lid].Real + "')", "showImageView_Fahrtverlauf('" + lid + "')", "showDetailView_ProblemListe('" + lid + "')", "handleSelectLineForVehicle(" + _S.B[lid].Li + ")"), new Array(1, 1, _S.B[lid].NextF, _S.B[lid].Real && Configuration.Skype, _S.B[lid].Real && Configuration.Weisungen, _S.B[lid].Real, 1, 1));
            registerContextMenu(window, "uml_" + lid, _K.U[lid]);
        }
    }
    _S.ZFahrzeuge = xmldoc.getElementsByTagName("stamp")[0].firstChild.data;
    changeZeitstempelTabelle(TABELLE_BETRIEBSLAGE, _S.ZFahrzeuge);
    if (_S.Modus == MODUS_TABELLE && getThemaTabelle() == TABELLE_BETRIEBSLAGE) {
        schreibeTabelle();
    } else {
        if (_S.Modus == MODUS_PLAN && _S.ThemaPlan != THEMA_DFI) {
            if (_S.ThemaPlan == THEMA_GIS) {
                GV_update();
            } else {
                if (_S.ThemaPlan == THEMA_SVG) {
                    SVG_update();
                } else {
                    Netzplan_update();
                }
            }
            var status2 = xmldoc.getElementsByTagName("status")[0].firstChild.data;
            if (sMitGisView) {
                document.getElementById("ID_status").firstChild.data = status2 + " / " + Netzplan_getAnzahlFahrzeuge() + " / " + GV_getAnzahlFahrzeuge() + "   " + sMyLanguage;
            } else {
                document.getElementById("ID_status").firstChild.data = status2 + " / " + Netzplan_getAnzahlFahrzeuge() + "   " + sMyLanguage;
            }
        }
    }
    delete (xmldoc);
}

function _showDfiData() {
    setzenThemaTabelle(TABELLE_ANZEIGEN);
    _aendernModus();
}

function _aendernModus(inEvent) {
    var event = (inEvent) ? inEvent : (top.map.event ? top.map.event : top.event);
    if (event) {
        event.cancelBubble = true;
    }
    hideContextMenu();
    if (_S.Modus == MODUS_PLAN) {
        _E.RegioView.style.visibility = "hidden";
        switchViews(false, false, false);
        window.scrollTo(0, 0);
        _S.Modus = MODUS_TABELLE;
        zeigenTabelle();
    } else {
        if (getThemaTabelle() != TABELLE_BETRIEBSLAGE && getThemaTabelle() != TABELLE_ANZEIGEN) {
            changeThemaTabelle(TABELLE_BETRIEBSLAGE);
        } else {
            changeThemaTabelle(TABELLE_BETRIEBSLAGE);
            verbergenTabelle();
            window.scrollTo(0, 0);
            _S.Modus = MODUS_PLAN;
            _E.RegioView.style.visibility = "visible";
            document.getElementById("ID_AnzeigenAufRahmen").style.visibility = "visible";
            if (_S.ThemaPlan == THEMA_GIS) {
                switchViews(false, true, false);
            } else {
                if (_S.ThemaPlan == THEMA_SVG) {
                    switchViews(false, false, true);
                } else {
                    switchViews(true, false, false);
                    Netzplan_update();
                }
            }
        }
    }
    return false;
}

function changeThemaPlan(aThema, aParameter) {
    if (_S.ThemaPlan != aThema || _S.ThemaPlanParamter != aParameter) {
        var vThemaPlanAlt = _S.ThemaPlan;
        _S.ThemaPlan = aThema;
        _S.ThemaPlanParamter = aParameter;
        deselektierenMenuPunkt(_E.MenuWerkzeugLinie);
        deselektierenMenuPunkt(_E.MenuWerkzeugFpa);
        deselektierenMenuPunkt(_E.MenuWerkzeugSvg);
        deselektierenMenuPunkt(_E.MenuWerkzeugGis);
        deselektierenMenuPunkt(_E.MenuWerkzeugDfi0);
        deselektierenMenuPunkt(_E.MenuWerkzeugDfi1);
        deselektierenMenuPunkt(_E.MenuWerkzeugDfi6);
        deselektierenMenuPunkt(_E.MenuWerkzeugDfi24);
        deselektierenMenuPunkt(_E.MenuWerkzeugDfi48);
        deselektierenMenuPunkt(_E.MenuWerkzeugSoftware);
        deselektierenMenuPunkt(_E.MenuWerkzeugDaten);
        if (_S.ThemaPlan == THEMA_DFI) {
            Netzplan_setzenInhaltAnzeigenStattFahrzeuge(1);
        } else {
            Netzplan_setzenInhaltAnzeigenStattFahrzeuge(0);
        }
        if (_S.ThemaPlan == THEMA_GIS) {
            if (!sMitGisView) {
                sMitGisView = GV_init(_E.GisView, document.getElementById("ID_time").firstChild, Configuration);
            }
            if (!sMitGisView) {
                loeschenObjekt(_E.MenuWerkzeugGis.parentNode);
                anzeigenFehler(getI18nMessage("GIS view not available. Please check the Internet configuration for your computer."));
                _S.ThemaPlan = THEMA_LINIEN;
                _E.MenuWerkzeugLinie.focus();
            } else {
                switchViews(false, true, false);
            }
        } else {
            if (_S.ThemaPlan == THEMA_SVG) {
                if (!sMitSvgView) {
                    sMitSvgView = SVG_init(_E.SvgView, document.getElementById("ID_time").firstChild, Configuration);
                }
                if (!sMitSvgView) {
                    loeschenObjekt(_E.MenuWerkzeugSvg.parentNode);
                    anzeigenFehler(getI18nMessage("SVG view not available. Please check the Internet configuration for your computer."));
                    _S.ThemaPlan = THEMA_LINIEN;
                    _E.MenuWerkzeugLinie.focus();
                } else {
                    switchViews(false, false, true);
                }
            } else {
                switchViews(true, false, false);
                Netzplan_update();
            }
        }
        if (_S.ThemaPlan == THEMA_DFI && _S.ALinie) {
            handleAuswahlLinie(0);
        }
        if (_S.ThemaPlan == THEMA_SOFTWARE) {
            _E.MenuWerkzeugSoftware.style.color = sColorSelected;
        } else {
            if (_S.ThemaPlan == THEMA_DATEN) {
                _E.MenuWerkzeugDaten.style.color = sColorSelected;
            } else {
                if (_S.ThemaPlan == THEMA_FPA) {
                    _E.MenuWerkzeugFpa.style.color = sColorSelected;
                } else {
                    if (_S.ThemaPlan == THEMA_SVG) {
                        _E.MenuWerkzeugSvg.style.color = sColorSelected;
                    } else {
                        if (_S.ThemaPlan == THEMA_GIS) {
                            _E.MenuWerkzeugGis.style.color = sColorSelected;
                        } else {
                            if (_S.ThemaPlan == THEMA_DFI) {
                                if (aParameter == 0) {
                                    _E.MenuWerkzeugDfi0.style.color = sColorSelected;
                                } else {
                                    if (aParameter == 1) {
                                        _E.MenuWerkzeugDfi1.style.color = sColorSelected;
                                    } else {
                                        if (aParameter == 2) {
                                            _E.MenuWerkzeugDfi6.style.color = sColorSelected;
                                        } else {
                                            if (aParameter == 3) {
                                                _E.MenuWerkzeugDfi24.style.color = sColorSelected;
                                            } else {
                                                if (aParameter == 4) {
                                                    _E.MenuWerkzeugDfi48.style.color = sColorSelected;
                                                }
                                            }
                                        }
                                    }
                                }
                            } else {
                                _E.MenuWerkzeugLinie.style.color = sColorSelected;
                            }
                        }
                    }
                }
            }
        }
    }
    return false;
}

function switchViews(net, gis, svg) {
    _E.SchemaView.style.visibility = (net) ? "visible" : "hidden";
    document.getElementById("ID_AnzeigenAufRahmen").style.display = (net) ? "block" : "none";
    (gis) ? GV_zeigen() : GV_verbergen();
    if (this.SVG_verbergen) {
        (svg) ? SVG_zeigen() : SVG_verbergen();
    }
}

function handleSelectLineForVehicle(aLinie) {
    if (aLinie != _S.ALinie) {
        handleAuswahlLinie(aLinie);
    } else {
        handleAuswahlLinie(0);
    }
}

function handleAuswahlLinie(aLinie) {
    if (_S.ALinie != aLinie) {
        if (_S.ALinie) {
            Netzplan_markiereRoute(_S.ALinie, 0);
            if (sMitGisView) {
                GV_markiereRoute(_S.ALinie, 0);
            }
        }
        _S.ALinie = aLinie;
        if (aLinie) {
            Netzplan_markiereRoute(_S.ALinie, 1);
            if (sMitGisView) {
                GV_markiereRoute(_S.ALinie, 1);
            }
        }
        changeKeyTabelle(TABELLE_BETRIEBSLAGE, aLinie);
        if (_S.Modus == MODUS_TABELLE && getThemaTabelle() == TABELLE_BETRIEBSLAGE) {
            schreibeTabelle();
        } else {
            if (_S.ThemaPlan != THEMA_GIS) {
                Netzplan_update();
            } else {
                if (sMitGisView) {
                    GV_update();
                }
            }
        }
    }
}

function _steuernAnzeige(aHastId, aAnzeigeId) {
    _S.AAnzeige = aAnzeigeId;
    Netzplan_update();
    try {
        _E.LabelStatus.data = "sending Datareload-Request to HAST..";
        sendCommandRequest("dpi_load_data", aHastId);
        _E.LabelStatus.data = "Datareload-Request to HAST sent";
    } catch(e) {
        alert("Could not send request to HAST " + aHastId);
    }
    window.setTimeout("_S.AAnzeige = 0;Netzplan_update()", 100);
}

function checkStammdaten() {
    if (sVersionStammdataIst[0] != sVersionStammdataSoll[0]) {
        sPromise = $.when(requestStammdaten("lines", "Liniendaten")).then(requestStammdaten("stations", "Haltestellendaten"));
    } else {
        if (sVersionStammdataIst[1] != sVersionStammdataSoll[1]) {
            requestStammdaten("vehicles", "Fahrzeugdaten");
        }
    }
}

function requestStammdaten(aDataType, aBezeichnung) {
    try {
        var promise = sendDataRequest(aDataType);
        sTimestampLastRequest[2] = new Date().getTime();
        return promise;
    } catch(e) {
        anzeigenFehler(aBezeichnung + " können nicht geladen werden. Versuchen Sie einen Reload.");
    }
}

function Line(lineId) {
    this.id = lineId;
    this.label = lineId;
    this.stations = [];
    this.destinations = null;
    this.color = c(lineId);
    this.setLabel = function(label) {
        this.label = label;
    };
    this.addStation = function(station) {
        this.stations.push(station);
    };
    this.setDestinations = function(destinations) {
        this.destinations = destinations;
    };
    function c(num) {
        tmp = ("0." + num) * 1;
        return "#" + ((1 << 24) * (tmp + 1) | 0).toString(16).substr(1);
    }

}

function leseStammdatenLinien(xmldoc) {
    getValueAndLabel = function(item) {
        var tmp = item.split("|");
        return {
            value : tmp[0],
            label : (tmp.length > 1) ? tmp[1] : tmp[0]
        };
    };
    var vLines = xmldoc.getElementsByTagName("row");
    for ( i = 0; i < vLines.length; i++) {
        var vLineObj = getValueAndLabel(vLines[i].getAttribute("a"));
        var vLineDestinations = vLines[i].getAttribute("b");
        if (!_D.L[vLineObj.value]) {
            _D.L[vLineObj.value] = new Line(vLineObj.value);
        }
        _D.L[vLineObj.value].setLabel(vLineObj.label);
        _D.L[vLineObj.value].setDestinations(vLineDestinations);
    }
    sVersionStammdataIst[2] = sVersionStammdataSoll[2];
}

function leseStammdatenHaltestellen(xmldoc) {
    resolveLineLabel = function(lines) {
        return $.map(lines, function(line) {
            return _D.L[line].label;
        }).join(" ");
    };
    if (xmldoc.getElementsByTagName("status")[0].firstChild.data) {
        _K.H = [];
        sDatenTabelle[TABELLE_HALTESTELLEN] = new Array();
        sDatenTabelle[TABELLE_ANZEIGEN] = new Array();
        var vStationen = xmldoc.getElementsByTagName("hst");
        for ( i = 0; i < vStationen.length; i++) {
            var vHstNummer = vStationen[i].getAttribute("no");
            var vHstName = convertChars(vStationen[i].getAttribute("na"));
            _D.H[vHstNummer] = {
                VHST : 0,
                Name : 0,
                Linien : "",
                Lon : 0,
                Lat : 0,
                Haltepunkte : 0,
                DPIs : 0
            };
            _D.H[vHstNummer].VHST = vStationen[i].getAttribute("vh");
            _D.H[vHstNummer].Name = vHstName ? vHstName : vHstNummer;
            _D.H[vHstNummer].Linien = vStationen[i].getAttribute("li");
            _D.H[vHstNummer].Lon = vStationen[i].getAttribute("lo");
            _D.H[vHstNummer].Lat = vStationen[i].getAttribute("la");
            _D.H[vHstNummer].DPIs = vStationen[i].getAttribute("dpi");
            _D.H[vHstNummer].Haltepunkte = vStationen[i].getAttribute("hp");
            _D.H[vHstNummer].Vdv = vStationen[i].getAttribute("vdv");
            _D.H[vHstNummer].hps = [];
            var vHaltepunkte = vStationen[i].childNodes;
            for ( j = 0; j < vHaltepunkte.length; j++) {
                var vMastId = vHaltepunkte[j].getAttribute("dId"), vDfiLon = vHaltepunkte[j].getAttribute("dLo"), vDfiLat = vHaltepunkte[j].getAttribute("dLa"), vDfiNummer = vHaltepunkte[j].getAttribute("dNu");
                if (vDfiLon > 0 && vDfiLat > 0) {
                    _D.H[vHstNummer].hps.push({
                        lon : vDfiLon,
                        lat : vDfiLat,
                        hpid : vMastId,
                        dpi : vDfiNummer
                    });
                }
                if (vDfiNummer) {
                    var vDfiName = convertChars(vHaltepunkte[j].getAttribute("dNa"));
                    var vHastNu = vHaltepunkte[j].getAttribute("dHN");
                    var vDfiLeft = vHaltepunkte[j].getAttribute("dLe");
                    var vDfiTop = vHaltepunkte[j].getAttribute("dTo");
                    if (!vDfiName) {
                        vDfiName = "ERROR";
                    }
                    if (sMapDfiNuTabelle[vDfiNummer] == null) {
                        sDatenTabelle[TABELLE_ANZEIGEN].push(new Array(vDfiNummer, vHastNu, vDfiName, "?", "?", "?", "?", "?", "?", vDfiLeft, vDfiTop));
                        sMapDfiNuTabelle[vDfiNummer] = sDatenTabelle[TABELLE_ANZEIGEN].length;
                    }
                }
            }
            var vLinienArray = _D.H[vHstNummer].Linien.split(" ");
            for ( j = 0; j < vLinienArray.length; j++) {
                var vLinie = vLinienArray[j];
                if (!_D.L[vLinie]) {
                    _D.L[vLinie] = new Line(vLinie);
                }
                _D.L[vLinie].addStation(vHstNummer);
            }
            var vData = new Array(vHstNummer, _D.H[vHstNummer].Name, resolveLineLabel(_D.H[vHstNummer].Linien.split(" ")), _D.H[vHstNummer].Haltepunkte, _D.H[vHstNummer].Lon ? _D.H[vHstNummer].Lon : "", _D.H[vHstNummer].Lat ? _D.H[vHstNummer].Lat : "");
            if (Configuration.Mandant) {
                vData.splice(2, 0, _D.H[vHstNummer].VHST);
            }
            if (Configuration.Dfi) {
                vData.push(_D.H[vHstNummer].DPIs ? _D.H[vHstNummer].DPIs : "-");
            }
            if (Configuration.Vdv) {
                vData.push(_D.H[vHstNummer].Vdv ? _D.H[vHstNummer].Vdv : "-");
            }
            sDatenTabelle[TABELLE_HALTESTELLEN].push(vData);
            _K.H[vHstNummer] = definiereContextMenu(vHstName + " (" + vHstNummer + ")", new Array(getI18nMessage("Departures"), getI18nMessage("Stops"), getI18nMessage("FIS commun")), new Array("showDetailView_AbfahrtsListe('" + vHstNummer + "')", "showDetailView_StopListe('" + vHstNummer + "')", "showView_Zdfi('" + vHstNummer + "')"), new Array(1, 1, 1));
            registerContextMenu(window, "hst_" + vHstNummer, _K.H[vHstNummer]);
        }
        if (sMitGisView) {
            GV_setHaltestellen();
        }
        Netzplan_erstellenContextMenuHaltestellen();
        var vNetzplanElemente = Netzplan_getNetzplanElemente();
        for ( i = 0; i < vNetzplanElemente.length; i++) {
            if (sMitGisView && vNetzplanElemente[i].id.substring(0, 7) == "pln_geb") {
                vNetzplanElemente[i].onclick = new Function("handleAuswahlGebiet('" + vNetzplanElemente[i].id.substring(7) + "')");
                vNetzplanElemente[i].style.cursor = "pointer";
            }
        }
        changeZeitstempelTabelle(TABELLE_HALTESTELLEN, sVersionStammdataSoll[0]);
        if (getThemaTabelle() == TABELLE_HALTESTELLEN) {
            schreibeTabelle();
        }
        sVersionStammdataIst[0] = sVersionStammdataSoll[0];
    }
}

function handleAuswahlGebiet(aHaupthaltestelle) {
}

function leseStammdatenFahrzeuge(xmldoc) {
    var vFahrzeuge = xmldoc.getElementsByTagName("row");
    for ( i = 0; i < vFahrzeuge.length; i++) {
        var vNummer = vFahrzeuge[i].getAttribute("a");
        sMapPhoneNumber[vNummer] = vFahrzeuge[i].getAttribute("b");
    }
    sVersionStammdataIst[1] = sVersionStammdataSoll[1];
}

function deselektierenMenuPunkt(vMenupunkt) {
    if (vMenupunkt) {
        vMenupunkt.style.color = "";
    }
}

function anrufenFahrzeug(aFahrzeugNummer) {
    var vNumber = sMapPhoneNumber[aFahrzeugNummer];
    if (!vNumber) {
        anzeigenFehler(getI18nMessage("Unknown phone number for vehicle") + " " + aFahrzeugNummer + ".");
    } else {
        RegioviewFunction_anrufenSkype(vNumber);
    }
}

function login() {
    var vMenupunkt = findenObjekt("ID_LinkLogin");
    vMenupunkt.style.color = sColorSelected;
    open("login.html", "", "dependent=yes");
    vMenupunkt.style.color = "";
}

function addFunktion(aElement, aFunktion) {
    findenObjekt(aElement).onclick = new Function(aFunktion);
    _E.MenuWerkzeugSoftware = findenObjekt("ID_karteThemaSoftware");
    _E.MenuWerkzeugDaten = findenObjekt("ID_karteThemaDaten");
}

function _selectLineOrService() {
    var vLineNumber = prompt(getI18nMessage("Enter a line number") + ":", _S.ALinie ? _S.ALinie : "");
    if (vLineNumber) {
        if ((!sRoute[vLineNumber]) && (!_D.L[vLineNumber])) {
            alert(getI18nMessage("The line number is invalid") + ": " + vLineNumber);
        } else {
            if (vLineNumber != _S.ALinie) {
                handleAuswahlLinie(vLineNumber);
            }
        }
    } else {
        handleAuswahlLinie(0);
    }
}

function showMandantenListe() {
    var path = window.location.pathname;
    window.location = path.substr(0, path.indexOf("/", 1)) + "/welcome";
}