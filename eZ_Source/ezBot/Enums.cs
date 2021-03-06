// Decompiled with JetBrains decompiler
// Type: ezBot.Enums
// Assembly: ezBot, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3420E614-A4AA-4D46-B920-F86DC62C4464
// Assembly location: C:\Users\Hesa\Desktop\eZ\ezBot.exe

using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;

namespace ezBot
{
    internal class Enums
    {
        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH!");
            return input.First().ToString().ToUpper() + input.Substring(1);
        }
        /*
        public static int GetChampion(string name)
        {
            string jsonData = "";
            try
            {
                name = FirstCharToUpper(name);
                WebClient webClient = new WebClient();
                jsonData = webClient.DownloadString(string.Format("http://ddragon.leagueoflegends.com/cdn/5.14.1/data/en_US/champion/{0}.json", name));
                if(!string.IsNullOrEmpty(jsonData) && !jsonData.Contains("Not Found"))
                {
                    JObject rss = JObject.Parse(jsonData);
                    string championId = rss.Last.Last.Last[1].Value<string>();
                    return Convert.ToInt32(championId);
                }else
                {
                    jsonData = "";
                }
            }catch(Exception e)
            {

            }
            if(!string.IsNullOrEmpty(jsonData))
            {
                JObject o2 = (JObject)JToken.Parse(jsonData);
                var championId = o2.SelectToken("data."+name+ ".key").Value<string>();
                return Convert.ToInt32(championId);
            }
            return 0;
        }*/

        public static int GetChampion(string name)
        {
            switch (name.ToUpper())
            {
                case "AATROX":
                return 266;
                case "AHRI":
                return 103;
                case "AKALI":
                return 84;
                case "ALISTAR":
                return 12;
                case "AMUMU":
                return 32;
                case "ANIVIA":
                return 34;
                case "ANNIE":
                return 1;
                case "ASHE":
                return 22;
                case "AURELIONSOL":
                return 136;
                case "AZIR":
                return 268;
                case "BARD":
                return 432;
                case "BLITZCRANK":
                return 53;
                case "BRAND":
                return 63;
                case "BRAUM":
                return 201;
                case "CAITLYN":
                return 51;
                case "CASSIOPEIA":
                return 69;
                case "CHOGATH":
                return 31;
                case "CORKI":
                return 42;
                case "DARIUS":
                return 122;
                case "DIANA":
                return 131;
                case "MUNDO":
                return 36;
                case "DRAVEN":
                return 119;
                case "EKKO":
                return 245;
                case "ELISE":
                return 60;
                case "EVELYNN":
                return 28;
                case "EZREAL":
                return 81;
                case "FIDDLESTICKS":
                return 9;
                case "FIORA":
                return 114;
                case "FIZZ":
                return 105;
                case "GALIO":
                return 3;
                case "GANGPLANK":
                return 41;
                case "GAREN":
                return 86;
                case "GNAR":
                return 150;
                case "GRAGAS":
                return 79;
                case "GRAVES":
                return 104;
                case "HECARIM":
                return 120;
                case "HEIMERDIGER":
                return 74;
                case "ILLAOI":
                return 420;
                case "IRELIA":
                return 39;
                case "IVERN":
                return 427;
                case "JANNA":
                return 40;
                case "JARVAN":
                return 59;
                case "JAX":
                return 24;
                case "JAYCE":
                return 126;
                case "JINX":
                return 222;
                case "JHIN":
                return 202;
                case "KALISTA":
                return 429;
                case "KARMA":
                return 43;
                case "KARTHUS":
                return 30;
                case "KASSADIN":
                return 38;
                case "KATARINA":
                return 55;
                case "KAYLE":
                return 10;
                case "KENNEN":
                return 85;
                case "KHAZIX":
                return 121;
                case "KINDRED":
                return 203;
                case "KOGMAW":
                return 96;
                case "KLED":
                return 240;
                case "LEBLANC":
                return 7;
                case "LEESIN":
                return 64;
                case "LEONA":
                return 89;
                case "LISSANDRA":
                return (int)sbyte.MaxValue;
                case "LUCIAN":
                return 236;
                case "LULU":
                return 117;
                case "LUX":
                return 99;
                case "MALPHITE":
                return 54;
                case "MALZAHAR":
                return 90;
                case "MAOKAI":
                return 57;
                case "MASTERYI":
                return 11;
                case "MISSFORTUNE":
                return 21;
                case "MORDEKAISER":
                return 82;
                case "MORGANA":
                return 25;
                case "NAMI":
                return 267;
                case "NASUS":
                return 75;
                case "NAUTILUS":
                return 111;
                case "NIDALEE":
                return 76;
                case "NOCTURNE":
                return 56;
                case "NUNU":
                return 20;
                case "OLAF":
                return 2;
                case "ORIANNA":
                return 61;
                case "PANTHEON":
                return 80;
                case "POPPY":
                return 78;
                case "QUINN":
                return 133;
                case "RAMMUS":
                return 33;
                case "REKSAI":
                return 421;
                case "RENEKTON":
                return 58;
                case "RENGAR":
                return 107;
                case "RIVEN":
                return 92;
                case "RUMBLE":
                return 68;
                case "RYZE":
                return 13;
                case "SEJUANI":
                return 113;
                case "SHACO":
                return 35;
                case "SHEN":
                return 98;
                case "SHYVANA":
                return 102;
                case "SINGED":
                return 27;
                case "SION":
                return 14;
                case "SIVIR":
                return 15;
                case "SKARNER":
                return 72;
                case "SONA":
                return 37;
                case "SORAKA":
                return 16;
                case "SWAIN":
                return 50;
                case "SYNDRA":
                return 134;
                case "TAHMKENCH":
                return 223;
                case "TALIYAH":
                return 163;
                case "TALON":
                return 91;
                case "TARIC":
                return 44;
                case "TEEMO":
                return 17;
                case "THRESH":
                return 412;
                case "TRISTANA":
                return 18;
                case "TRUNDLE":
                return 48;
                case "TRYNDAMERE":
                return 23;
                case "TWISTEDFATE":
                return 4;
                case "TWITCH":
                return 29;
                case "UDYR":
                return 77;
                case "URGOT":
                return 6;
                case "VARUS":
                return 110;
                case "VAYNE":
                return 67;
                case "VEIGAR":
                return 45;
                case "VELKOZ":
                return 161;
                case "VI":
                return 254;
                case "VIKTOR":
                return 112;
                case "VLADIMIR":
                return 8;
                case "VOLIBEAR":
                return 106;
                case "WARWICK":
                return 19;
                case "WUKONG":
                return 62;
                case "XERATH":
                return 101;
                case "XINZHAO":
                return 5;
                case "YASUO":
                return 157;
                case "YORICK":
                return 83;
                case "ZAC":
                return 154;
                case "ZED":
                return 238;
                case "ZIGGS":
                return 115;
                case "ZILEAN":
                return 26;
                case "ZYRA":
                return 143;
                default:
                return 0;
            }
        }

        public static string GetChampionById(int championId)
        {
            switch(championId)
            {
                case 266:
                return "AATROX";
                case 103:
                return "AHRI";
                case 84:
                return "AKALI";
                case 12:
                return "ALISTAR";
                case 32:
                return "AMUMU";
                case 34:
                return "ANIVIA";
                case 1:
                return "ANNIE";
                case 22:
                return "ASHE";
                case 136:
                return "AURELIONSOL";
                case 268:
                return "AZIR";
                case 432:
                return "BARD";
                case 53:
                return "BLITZCRANK";
                case 63:
                return "BRAND";
                case 201:
                return "BRAUM";
                case 51:
                return "CAITLYN";
                case 69:
                return "CASSIOPEIA";
                case 31:
                return "CHOGATH";
                case 42:
                return "CORKI";
                case 122:
                return "DARIUS";
                case 131:
                return "DIANA";
                case 36:
                return "MUNDO";
                case 119:
                return "DRAVEN";
                case 245:
                return "EKKO";
                case 60:
                return "ELISE";
                case 28:
                return "EVELYNN";
                case 81:
                return "EZREAL";
                case 9:
                return "FIDDLESTICKS";
                case 114:
                return "FIORA";
                case 105:
                return "FIZZ";
                case 3:
                return "GALIO";
                case 41:
                return "GANGPLANK";
                case 86:
                return "GAREN";
                case 150:
                return "GNAR";
                case 79:
                return "GRAGAS";
                case 104:
                return "GRAVES";
                case 120:
                return "HECARIM";
                case 74:
                return "HEIMERDIGER";
                case 420:
                return "ILLAOI";
                case 39:
                return "IRELIA";
                case 427:
                return "IVERN";
                case 40:
                return "JANNA";
                case 59:
                return "JARVAN";
                case 24:
                return "JAX";
                case 126:
                return "JAYCE";
                case 222:
                return "JINX";
                case 202:
                return "JHIN";
                case 429:
                return "KALISTA";
                case 43:
                return "KARMA";
                case 30:
                return "KARTHUS";
                case 38:
                return "KASSADIN";
                case 55:
                return "KATARINA";
                case 10:
                return "KAYLE";
                case 85:
                return "KENNEN";
                case 121:
                return "KHAZIX";
                case 203:
                return "KINDRED";
                case 96:
                return "KOGMAW";
                case 240:
                return "KLED";
                case 7:
                return "LEBLANC";
                case 64:
                return "LEESIN";
                case 89:
                return "LEONA";
                case (int)sbyte.MaxValue:
                return "LISSANDRA";
                case 236:
                return "LUCIAN";
                case 117:
                return "LULU";
                case 99:
                return "LUX";
                case 54:
                return "MALPHITE";
                case 90:
                return "MALZAHAR";
                case 57:
                return "MAOKAI";
                case 11:
                return "MASTERYI";
                case 21:
                return "MISSFORTUNE";
                case 82:
                return "MORDEKAISER";
                case 25:
                return "MORGANA";
                case 267:
                return "NAMI";
                case 75:
                return "NASUS";
                case 111:
                return "NAUTILUS";
                case 76:
                return "NIDALEE";
                case 56:
                return "NOCTURNE";
                case 20:
                return "NUNU";
                case 2:
                return "OLAF";
                case 61:
                return "ORIANNA";
                case 80:
                return "PANTHEON";
                case 78:
                return "POPPY";
                case 133:
                return "QUINN";
                case 33:
                return "RAMMUS";
                case 421:
                return "REKSAI";
                case 58:
                return "RENEKTON";
                case 107:
                return "RENGAR";
                case 92:
                return "RIVEN";
                case 68:
                return "RUMBLE";
                case 13:
                return "RYZE";
                case 113:
                return "SEJUANI";
                case 35:
                return "SHACO";
                case 98:
                return "SHEN";
                case 102:
                return "SHYVANA";
                case 27:
                return "SINGED";
                case 14:
                return "SION";
                case 15:
                return "SIVIR";
                case 72:
                return "SKARNER";
                case 37:
                return "SONA";
                case 16:
                return "SORAKA";
                case 50:
                return "SWAIN";
                case 134:
                return "SYNDRA";
                case 223:
                return "TAHMKENCH";
                case 163:
                return "TALIYAH";
                case 91:
                return "TALON";
                case 44:
                return "TARIC";
                case 17:
                return "TEEMO";
                case 412:
                return "THRESH";
                case 18:
                return "TRISTANA";
                case 48:
                return "TRUNDLE";
                case 23:
                return "TRYNDAMERE";
                case 4:
                return "TWISTEDFATE";
                case 29:
                return "TWITCH";
                case 77:
                return "UDYR";
                case 6:
                return "URGOT";
                case 110:
                return "VARUS";
                case 67:
                return "VAYNE";
                case 45:
                return "VEIGAR";
                case 161:
                return "VELKOZ";
                case 254:
                return "VI";
                case 112:
                return "VIKTOR";
                case 8:
                return "VLADIMIR";
                case 106:
                return "VOLIBEAR";
                case 19:
                return "WARWICK";
                case 62:
                return "WUKONG";
                case 101:
                return "XERATH";
                case 5:
                return "XINZHAO";
                case 157:
                return "YASUO";
                case 83:
                return "YORICK";
                case 154:
                return "ZAC";
                case 238:
                return "ZED";
                case 115:
                return "ZIGGS";
                case 26:
                return "ZILEAN";
                case 143:
                return "ZYRA";
                case 0:
                return "ZYRA";
                default:
                return "";
            }
        }

        public static int GetSpell(string name)
        {
            switch(name.ToUpper())
            {
                case "POROTHROW":
                    return 31;
                break;
                case "SMITE":
                    return 11;
                break;
                case "GHOST":
                    return 6;
                break;
                case "IGNITE":
                    return 14;
                break;
                case "HEAL":
                    return 7;
                break;
                case "TELEPORT":
                    return 12;
                break;
                case "GARRISON":
                    return 17;
                break;
                case "CLAIRVOYANCE":
                    return 2;
                break;
                case "EXHAUST":
                    return 3;
                break;
                case "BARRIER":
                    return 21;
                break;
                case "SNOWBALL":
                    return 11;
                break;
                case "FLASH":
                    return 4;
                break;
                case "REVIVE":
                    return 10;
                break;
                case "PORORECALL":
                    return 30;
                break;
                case "CLARITY":
                    return 13;
                break;
                case "CLEANSE":
                    return 1;
                break;
            }
            return 0;
        }
    }
}