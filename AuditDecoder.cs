namespace SQLAuditWatcherJsonService;

using Microsoft.SqlServer.XEvent.XELite;
using System.Globalization;
using System.IO;

public static class AuditDecoder
{
    private static readonly Dictionary<int, string> LookupValues = new()
    {
[1329873729] = "ACDO",
[542065473] = "ACO ",
[1329742913] = "ADBO",
[1346651201] = "ADDP",
[1129530433] = "ADSC",
[1347634241] = "ADSP",
[538987585] = "AL  ",
[1313033281] = "ALCN",
[1380731969] = "ALLR",
[1129466945] = "ALRC",
[1397902401] = "ALRS",
[1397967937] = "ALSS",
[1414745153] = "ALST",
[1381256257] = "ALTR",
[1280462913] = "APRL",
[538989377] = "AS  ",
[1129534785] = "AUSC",
[1179866433] = "AUSF",
[1213486401] = "AUTH",
[538984770] = "BA  ",
[541868354] = "BAL ",
[541934402] = "BCM ",
[1196245826] = "BCMG",
[1111773762] = "BRDB",
[542397250] = "BST ",
[1196708674] = "BSTG",
[1179595331] = "C2OF",
[1313813059] = "C2ON",
[1196180291] = "CCLG",
[1196182851] = "CMLG",
[1430343235] = "CNAU",
[538988355] = "CO  ",
[538988611] = "CP  ",
[538989123] = "CR  ",
[538976324] = "D   ",
[1329742148] = "DABO",
[1179074884] = "DAGF",
[1279738180] = "DAGL",
[1397178692] = "DAGS",
[1178681924] = "DBAF",
[1396785732] = "DBAS",
[1128481348] = "DBCC",
[1195590212] = "DBCG",
[541868612] = "DBL ",
[538987588] = "DL  ",
[1280462916] = "DPRL",
[538989124] = "DR  ",
[1129534020] = "DRSC",
[541284164] = "DWC ",
[1396982085] = "EADS",
[1397113157] = "EAFS",
[1346651973] = "EGDP",
[1346783045] = "EGFP",
[1196377925] = "EGOG",
[1179928389] = "EGTF",
[1330923333] = "EGTO",
[1380732485] = "ENLR",
[538990661] = "EX  ",
[538989638] = "FT  ",
[541545542] = "FTG ",
[1279547206] = "FWDL",
[1146443590] = "FWUD",
[538976327] = "G   ",
[1380729927] = "GDLR",
[1111773767] = "GRDB",
[1329877575] = "GRDO",
[542069319] = "GRO ",
[1330860615] = "GRSO",
[1448301127] = "GRSV",
[541546311] = "GWG ",
[1346653513] = "IMDP",
[542133577] = "IMP ",
[1347636553] = "IMSP",
[538988105] = "IN  ",
[541214540] = "LGB ",
[1195525964] = "LGBG",
[1094993740] = "LGDA",
[1111770956] = "LGDB",
[1095059276] = "LGEA",
[1279674188] = "LGFL",
[541542220] = "LGG ",
[1195853644] = "LGGG",
[1179207500] = "LGIF",
[1397311308] = "LGIS",
[1196181324] = "LGLG",
[541935436] = "LGM ",
[1196246860] = "LGMG",
[1296975692] = "LGNM",
[542066508] = "LGO ",
[542328652] = "LGS ",
[1146308428] = "LGSD",
[1196640076] = "LGSG",
[538988364] = "LO  ",
[1111772749] = "MNDB",
[1329876557] = "MNDO",
[1346653773] = "MNDP",
[542068301] = "MNO ",
[1330859597] = "MNSO",
[1347636813] = "MNSP",
[1196182862] = "NMLG",
[538988623] = "OP  ",
[1111773263] = "OPDB",
[1380733007] = "OPLR",
[1448300623] = "OPSV",
[1380013904] = "PWAR",
[541284176] = "PWC ",
[1195595600] = "PWCG",
[1396922192] = "PWCS",
[1480939344] = "PWEX",
[1129142096] = "PWMC",
[1280333648] = "PWPL",
[542267216] = "PWR ",
[1397905232] = "PWRS",
[542463824] = "PWU ",
[538976338] = "R   ",
[538985298] = "RC  ",
[541934418] = "RCM ",
[538986066] = "RF  ",
[538989394] = "RS  ",
[542397266] = "RST ",
[541284178] = "RWC ",
[541546322] = "RWG ",
[541278803] = "SBC ",
[1195590227] = "SBCG",
[1195590483] = "SCCG",
[538987603] = "SL  ",
[538988115] = "SN  ",
[1313624147] = "SPLN",
[541282899] = "SRC ",
[1448301651] = "STSV",
[1313953107] = "SUQN",
[1313035859] = "SVCN",
[1146115667] = "SVPD",
[1146312275] = "SVSD",
[1381193299] = "SVSR",
[1095975252] = "TASA",
[1347633492] = "TASP",
[538988372] = "TO  ",
[1111773012] = "TODB",
[1329876820] = "TODO",
[542068564] = "TOO ",
[1330859860] = "TOSO",
[1128419924] = "TRBC",
[1396855380] = "TRBS",
[1128485460] = "TRCC",
[1195594324] = "TRCG",
[1396920916] = "TRCS",
[1128747604] = "TRGC",
[1397183060] = "TRGS",
[542069332] = "TRO ",
[1129337428] = "TRPC",
[1397772884] = "TRPS",
[1129468500] = "TRRC",
[1397903956] = "TRRS",
[1129534036] = "TRSC",
[1397969492] = "TRSS",
[538990676] = "TX  ",
[1195530324] = "TXBG",
[1195595860] = "TXCG",
[1296259156] = "TXCM",
[1195858004] = "TXGG",
[1112692820] = "TXRB",
[1196578900] = "TXRG",
[1346847573] = "UCGP",
[1195459669] = "UDAG",
[1430340693] = "UDAU",
[1195658837] = "UNDG",
[1329876565] = "UNDO",
[538988629] = "UP  ",
[1178686293] = "USAF",
[1196184405] = "USLG",
[1129599829] = "USTC",
[1414743126] = "VDST",
[1380730454] = "VFLR",
[1414746966] = "VSST",
[538990422] = "VW  ",
[1413699414] = "VWCT",
[538984792] = "XA  ",
[538989912] = "XU  ",
[8257] = "A ",
[8259] = "C ",
[8260] = "D ",
[8262] = "F ",
[8272] = "P ",
[8274] = "R ",
[8275] = "S ",
[8276] = "T ",
[8277] = "U ",
[8278] = "V ",
[8280] = "X ",
[16708] = "DA",
[16716] = "LA",
[16723] = "SA",
[16724] = "TA",
[16964] = "DB",
[16975] = "OB",
[17220] = "DC",
[17221] = "EC",
[17222] = "FC",
[17232] = "PC",
[17235] = "SC",
[17475] = "CD",
[17477] = "ED",
[17491] = "SD",
[17732] = "DE",
[17747] = "SE",
[17985] = "AF",
[17989] = "EF",
[17993] = "IF",
[18000] = "PF",
[18002] = "RF",
[18004] = "TF",
[18241] = "AG",
[18258] = "RG",
[18259] = "SG",
[18263] = "WG",
[19013] = "EJ",
[19265] = "AK",
[19267] = "CK",
[19268] = "DK",
[19277] = "MK",
[19280] = "PK",
[19283] = "SK",
[19521] = "AL",
[19523] = "CL",
[19525] = "EL",
[19526] = "FL",
[19536] = "PL",
[19538] = "RL",
[19539] = "SL",
[19543] = "WL",
[19544] = "XL",
[19779] = "CM",
[20034] = "BN",
[20036] = "DN",
[20037] = "EN",
[20038] = "FN",
[20047] = "ON",
[20051] = "SN",
[20291] = "CO",
[20307] = "SO",
[20545] = "AP",
[20547] = "CP",
[20549] = "EP",
[20550] = "FP",
[20563] = "SP",
[20801] = "AQ",
[20816] = "PQ",
[20819] = "SQ",
[20821] = "UQ",
[21057] = "AR",
[21059] = "CR",
[21060] = "DR",
[21061] = "ER",
[21072] = "PR",
[21075] = "SR",
[21076] = "TR",
[21080] = "XR",
[21313] = "AS",
[21316] = "DS",
[21317] = "ES",
[21318] = "FS",
[21321] = "IS",
[21328] = "PS",
[21333] = "US",
[21571] = "CT",
[21572] = "DT",
[21574] = "FT",
[21577] = "IT",
[21581] = "MT",
[21586] = "RT",
[21587] = "ST",
[21825] = "AU",
[21827] = "CU",
[21828] = "DU",
[21831] = "GU",
[21840] = "PU",
[21843] = "SU",
[21847] = "WU",
[21848] = "XU",
[22099] = "SV",
[22597] = "EX",
[22601] = "IX",
[22604] = "LX",
[22611] = "SX",
[22868] = "TY",
    };
    public static IReadOnlyList<Dictionary<string, string>> Decode(Stream stream, string path)
    {
        var xeStream = new XEFileEventStreamer(stream);
        var rows = new List<Dictionary<string, string>>();

        xeStream.ReadEventStream(
            xevent =>
            {
                var row = new Dictionary<string, string>
                {
                    ["event_name"] = xevent.Name,
                    ["timestamp"] = xevent.Timestamp.ToString("O")
                };

                foreach (var kv in xevent.Fields)
                {
                    var key = kv.Key;
                    var val = kv.Value;
                    string value;
                    if (key == "permission_bitmask" || key == "server_principal_sid" || key == "target_server_principal_sid")
                    {
                        if (val is byte[] bytes && bytes.Length > 0)
                            value = "0x" + BitConverter.ToString(bytes).Replace("-", string.Empty);
                        else
                            value = string.Empty;
                    }
                    else if (key == "sequence_group_id")
                    {
                        if (val is Guid guid)
                            value = "0x" + string.Concat(guid.ToByteArray().Select(b => b.ToString("x2"))).ToUpperInvariant();
                        else if (Guid.TryParse(val?.ToString(), out var g))
                            value = "0x" + string.Concat(g.ToByteArray().Select(b => b.ToString("x2"))).ToUpperInvariant();
                        else
                            value = val?.ToString() ?? string.Empty;
                    }
                    else if (key == "action_id" || key == "class_type")
                    {
                        if (val is int i && LookupValues.TryGetValue(i, out var mapped))
                            value = mapped;
                        else if (int.TryParse(val?.ToString(), out var i2) && LookupValues.TryGetValue(i2, out mapped))
                            value = mapped;
                        else
                            value = val?.ToString() ?? string.Empty;
                    }
                    else if (key == "event_time")
                    {
                        if (val is DateTime dt)
                            value = dt.ToString("yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture);
                        else if (DateTime.TryParse(val?.ToString(), out var dt2))
                            value = dt2.ToString("yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture);
                        else
                            value = val?.ToString() ?? string.Empty;
                    }
                    else
                    {
                        value = val?.ToString() ?? string.Empty;
                    }

                    row[key] = value;
                }

                row["file_name"] = path;
                row["audit_file_offset"] = string.Empty;

                rows.Add(row);
                return Task.CompletedTask;
            },
            CancellationToken.None).GetAwaiter().GetResult();

        return rows;
    }
}
