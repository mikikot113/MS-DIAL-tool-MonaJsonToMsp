using NCDK.Graphs.InChI;
using NCDK.Smiles;
using NCDK.Tools.Manipulator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NCDK.Default;

namespace MonaJsonToMsp.Common
{
    class program
    {
        public static void run(string workingDirectry, string[] ontologyList, string ExportDirectry, bool emptyIsPos, bool posNegSame)
        {
            var inportFiles = Directory.GetFiles(workingDirectry, "*.json");
            var ontologyDic = new Dictionary<string, string>();
            if (ontologyList != null)
            {
                ontologyDic = OntorogyDic(ontologyList);
            }
            var noOntology = new List<string>();
            var noData = new List<string>();

            foreach (var inportFile in inportFiles)
            {
                var monaJson = ParseJson(inportFile, ExportDirectry);
                var mspStorage = monaJson.Select(msp => setMsp(msp, ontologyDic));
                var ListPosNeg = new List<string>();
                var ListPos = new List<string>();
                var ListNeg = new List<string>();

                foreach (var msp in mspStorage)
                {
                    if (msp != null)
                    {
                        if (msp.Formula == null || msp.Formula == "")
                        {
                            noData.Add(Path.GetFileNameWithoutExtension(inportFile) + "\t" + msp.Name + "\t" + msp.Ionmode + "\t" + msp.Formula + "\t" + msp.Smiles + "\t" + msp.InchiKey);
                            continue;
                        }
                        if (posNegSame && emptyIsPos)
                        {
                            MspFieldsToList(msp, ListPosNeg);
                        }
                        else if (posNegSame && !emptyIsPos)
                        {
                            if (msp.Ionmode != null)
                            {
                                MspFieldsToList(msp, ListPosNeg);
                            }
                            else
                            {
                                noData.Add(Path.GetFileNameWithoutExtension(inportFile) + "\t" + msp.Name + "\t" + msp.Ionmode + "\t" + msp.Formula + "\t" + msp.Smiles + "\t" + msp.InchiKey);
                            }
                        }
                        else if (!posNegSame && emptyIsPos)
                        {
                            if (msp.Ionmode == null || msp.Ionmode.ToUpper().StartsWith("P"))
                            {
                                MspFieldsToList(msp, ListPos);
                            }
                            else if (msp.Ionmode.ToUpper().StartsWith("N"))
                            {
                                MspFieldsToList(msp, ListNeg);
                            }
                            else
                            {
                                noData.Add(Path.GetFileNameWithoutExtension(inportFile) + "\t" + msp.Name + "\t" + msp.Ionmode + "\t" + msp.Formula + "\t" + msp.Smiles + "\t" + msp.InchiKey);
                            }
                        }
                        else if (!posNegSame && !emptyIsPos)
                        {
                            if (msp.Ionmode != null && msp.Ionmode.ToUpper().StartsWith("P"))
                            {
                                MspFieldsToList(msp, ListPos);
                            }
                            else if (msp.Ionmode != null && msp.Ionmode.ToUpper().StartsWith("N"))
                            {
                                MspFieldsToList(msp, ListNeg);
                            }
                            else
                            {
                                noData.Add(Path.GetFileNameWithoutExtension(inportFile) + "\t" + msp.Name + "\t" + msp.Ionmode + "\t" + msp.Formula + "\t" + msp.Smiles + "\t" + msp.InchiKey);
                            }
                        }
                    }

                }

                File.WriteAllLines(ExportDirectry + "\\" + Path.GetFileNameWithoutExtension(inportFile) + "_posneg.msp", ListPosNeg);
                File.WriteAllLines(ExportDirectry + "\\" + Path.GetFileNameWithoutExtension(inportFile) + "_pos.msp", ListPos);
                File.WriteAllLines(ExportDirectry + "\\" + Path.GetFileNameWithoutExtension(inportFile) + "_neg.msp", ListNeg);
            }
            if (noData.Count > 0)
            {
                using (var sw = new StreamWriter(ExportDirectry + "\\lackingDataRecord.txt", false, Encoding.ASCII))
                {
                    sw.WriteLine("file\tName\tIonmode\tFormula\tSmiles\tInchiKey");
                    foreach (var item in noData)
                    {
                        sw.WriteLine(item);
                    }
                }
            }
        }

        static List<MonaJson> ParseJson(string input, string ExportDirectry)
        {
            var file = new StreamReader(input);
            string? line;
            var jsonData = new List<MonaJson>();
            var errorLine = new List<string>();

            while ((line = file.ReadLine()) != null)
            {
                var json = line;
                if (json.Length < 2) continue;
                var serializer = new DataContractJsonSerializer(typeof(MonaJson));

                var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
                ms.Seek(0, SeekOrigin.Begin);
                try
                {
                    var data = serializer.ReadObject(ms) as MonaJson;
                    jsonData.Add(data);
                }
                catch
                { errorLine.Add(json); }
            }
            if (errorLine.Count > 0)
            {
                File.WriteAllLines(Path.GetDirectoryName(ExportDirectry) + "\\CannotExchengedJson.txt", errorLine);
            }

            return jsonData;
        }

        private static MspStorage setMsp(MonaJson MonaJson, Dictionary<string, string> ontologyDic)
        {
            var mspStorage = new MspStorage();
            var jsonMeta = MonaJson.metaData;
            if (jsonMeta != null && MonaJson.compound != null)
            {
                var jsonCompound = MonaJson.compound[0];
                mspStorage.Name = jsonCompound?.names?[0].name;

                mspStorage.InstrumentType = jsonMeta.Where(n => n.name == "instrument type").Select(m => m.value).FirstOrDefault();
                //mspStorage.Instrument = jsonMeta.Where(n => n.name == "instrument").Select(m => m.value).FirstOrDefault();
                mspStorage.CollisionEnergy = jsonMeta.Where(n => n.name == "collision energy").Select(m => m.value).FirstOrDefault();
                mspStorage.Ionization = jsonMeta.Where(n => n.name == "ionization").Select(m => m.value).FirstOrDefault();

                mspStorage.PrecursorMz = jsonMeta.Where(n => n.name == "precursor m/z").Select(m => m.value).FirstOrDefault();
                mspStorage.PrecursorType = jsonMeta.Where(n => n.name == "precursor type" || n.name == "adduct").Select(m => m.value).FirstOrDefault();
                mspStorage.Ionmode = jsonMeta.Where(n => n.name == "ionization mode" || n.name == "ion mode").Select(m => m.value).FirstOrDefault();
                if (mspStorage.Ionmode == null || mspStorage.Ionmode == "")
                {
                    if (mspStorage.PrecursorType != null)
                    {
                        if (mspStorage.PrecursorType.Substring(mspStorage.PrecursorType.Length - 1) == "-")
                        {
                            mspStorage.Ionmode = "Negative";
                        }
                        else if (mspStorage.PrecursorType.Substring(mspStorage.PrecursorType.Length - 1) == "+")
                        {
                            mspStorage.Ionmode = "Positive";
                        }
                    }
                    //else
                    //{
                    //    mspStorage.Ionmode = "unknown"; // no export
                    //}
                }
                else if (mspStorage.Ionmode.ToUpper().StartsWith("N"))
                {
                    mspStorage.Ionmode = "Negative";
                }
                else if (mspStorage.Ionmode.ToUpper().StartsWith("P"))
                {
                    mspStorage.Ionmode = "Positive";
                }
                var retentiontimeRaw = jsonMeta.Where(n => n.name == "retention time").Select(m => m.value).FirstOrDefault();
                var inchi = jsonCompound?.inchi ?? "";
                mspStorage.Smiles = jsonCompound?.metaData?.Where(n => n.name == "SMILES").Select(m => m.value).FirstOrDefault();
                try
                {
                    var SmilesParser = new SmilesParser();
                    var iAtomContainer = SmilesParser.ParseSmiles(mspStorage.Smiles);
                }
                catch (Exception)
                {
                    try
                    {
                        var intostruct = InChIToStructure.FromInChI(inchi, ChemObjectBuilder.Instance);
                        var iAtomContainer = intostruct.AtomContainer;
                        var SmilesGenerator = new SmilesGenerator(SmiFlavors.Isomeric);
                        mspStorage.Smiles = SmilesGenerator.Create(iAtomContainer);
                    }
                    catch (Exception)
                    {
                        mspStorage.Smiles = "";
                    }
                }

                mspStorage.Formula = jsonCompound?.metaData?.Where(n => n.name == "molecular formula").Select(m => m.value).FirstOrDefault();
                if (mspStorage.Formula == null || mspStorage.Formula == "")
                {
                    if (mspStorage.Smiles != null || mspStorage.Smiles != "")
                    {
                        try
                        {
                            var SmilesParser = new SmilesParser();
                            var iAtomContainer = SmilesParser.ParseSmiles(mspStorage.Smiles);
                            var formula = MolecularFormulaManipulator.GetMolecularFormula(iAtomContainer);
                            mspStorage.Formula = MolecularFormulaManipulator.GetString(formula);
                        }
                        catch { }
                    }
                    else if (inchi != null)
                    {
                        var intostruct = InChIToStructure.FromInChI(inchi, ChemObjectBuilder.Instance);
                        var iAtomContainer = intostruct.AtomContainer;
                        var formula = MolecularFormulaManipulator.GetMolecularFormula(iAtomContainer);
                        mspStorage.Formula = MolecularFormulaManipulator.GetString(formula);
                    }
                }
                mspStorage.Exactmass = jsonCompound?.metaData?.Where(n => n.name == "total exact mass").Select(m => m.value).FirstOrDefault();
                mspStorage.InchiKey = jsonCompound?.metaData?.Where(n => n.name == "InChIKey").Select(m => m.value).FirstOrDefault();
                if (mspStorage.InchiKey == null || mspStorage.InchiKey == "")
                {
                    try
                    {
                        var SmilesParser = new SmilesParser();
                        var iAtomContainer = SmilesParser.ParseSmiles(mspStorage.Smiles);
                        var InChIGeneratorFactory = new InChIGeneratorFactory();
                        mspStorage.InchiKey = InChIGeneratorFactory.GetInChIGenerator(iAtomContainer).GetInChIKey();
                    }
                    catch
                    {
                        try
                        {
                            var intostruct = InChIToStructure.FromInChI(inchi, ChemObjectBuilder.Instance);
                            var iAtomContainer = intostruct.AtomContainer;
                            var InChIGeneratorFactory = new InChIGeneratorFactory();
                            mspStorage.InchiKey = InChIGeneratorFactory.GetInChIGenerator(iAtomContainer).GetInChIKey();
                        }
                        catch { }
                    }
                }

                if (MonaJson.spectrum != null)
                {
                    mspStorage.Peaks = setPeaks(MonaJson.spectrum);
                    mspStorage.Peaknum = mspStorage.Peaks.Count().ToString();
                }

                var jsonComment01 = MonaJson.id; //MoNA ID
                                                 //var jsonComment02 = MonaJson.library.description ??""; //library
                mspStorage.Comment = "DB#=" + jsonComment01;// + "; origin=" + jsonComment02;

                if (mspStorage.InchiKey != null)
                {
                    var shortInChIKey = mspStorage.InchiKey.Split('-')[0];
                    if (ontologyDic.ContainsKey(shortInChIKey))
                    {
                        mspStorage.Ontology = ontologyDic[shortInChIKey];
                    }
                }
                else if (jsonCompound?.classification != null)
                {
                    ///MoNA origin Ontology
                    mspStorage.Ontology = jsonCompound.classification.Where(n => n.name == "direct parent").Select(n => n.value).FirstOrDefault();
                }
            }

            return mspStorage;
        }

        private static List<MspPeak> setPeaks(string input)
        {
            var peaks = new List<MspPeak>();
            var peakItem = input.Split(';', ' ');
            var maxIntensityValue = peakItem.Select(n => double.Parse(n.Split(':')[1].Trim())).Max();
            var normalizationFactor = 1.0;
            if (maxIntensityValue != 100.0)
            {
                normalizationFactor = 100.0 / maxIntensityValue;
            }
            foreach (var item in peakItem)
            {
                var mz = Math.Round(double.Parse(item.Split(':')[0].Trim()), 3);
                var intensity = Math.Round(double.Parse(item.Split(':')[1].Trim()) * normalizationFactor * 10);
                if (intensity < 10)
                {
                    continue;
                }
                if (intensity == 999)
                {
                    intensity = 1000;
                }
                var peak = new MspPeak()
                {
                    Mz = mz,
                    Intensity = intensity,
                };
                peaks.Add(peak);
            }
            peaks = peaks.OrderBy(n => n.Mz).ToList();
            return peaks;
        }

        static Dictionary<string, string> OntorogyDic(string[] input)
        {
            var ontologyDic = new Dictionary<string, string>();
            foreach (var item in input)
            {
                if (item == "" || item == null) continue;
                using (var sr = new StreamReader(item, true))
                {
                    while (sr.Peek() > -1)
                    {
                        var line = sr.ReadLine();
                        if (line == null || !line.Contains('\t')) continue;
                        var lineArray = line.Split('\t');
                        var shortInChIKey = lineArray[0].Split('-')[0];
                        if (!ontologyDic.ContainsKey(shortInChIKey))
                        {
                            ontologyDic[shortInChIKey] = lineArray[1];
                        }
                    }
                }
            }
            return ontologyDic;
        }


        public static void writeMspFields(MspStorage mspStorage, StreamWriter sw)
        {
            sw.WriteLine("NAME: " + mspStorage.Name);
            sw.WriteLine("PRECURSORMZ: " + mspStorage.PrecursorMz);
            sw.WriteLine("PRECURSORTYPE: " + mspStorage.PrecursorType);
            sw.WriteLine("IONMODE: " + mspStorage.Ionmode);
            if (!string.IsNullOrEmpty(mspStorage.Formula))
            {
                sw.WriteLine("FORMULA: " + mspStorage.Formula);
            }
            if (!string.IsNullOrEmpty(mspStorage.Smiles))
            {
                sw.WriteLine("SMILES: " + mspStorage.Smiles);
            }
            //sw.WriteLine("INCHI: " + mspStorage.Inchi);
            if (!string.IsNullOrEmpty(mspStorage.InchiKey))
            {
                sw.WriteLine("INCHIKEY: " + mspStorage.InchiKey);
            }
            if (!string.IsNullOrEmpty(mspStorage.Ionization))
            {
                sw.WriteLine("IONIZATION: " + mspStorage.Ionization);
            }
            if (!string.IsNullOrEmpty(mspStorage.InstrumentType))
            {
                sw.WriteLine("INSTRUMENTTYPE: " + mspStorage.InstrumentType);
            }
            if (!string.IsNullOrEmpty(mspStorage.Instrument))
            {
                sw.WriteLine("INSTRUMENT: " + mspStorage.Instrument);
            }
            if (!string.IsNullOrEmpty(mspStorage.CollisionEnergy))
            {
                sw.WriteLine("COLLISIONENERGY: " + mspStorage.CollisionEnergy);
            }
            if (!string.IsNullOrEmpty(mspStorage.Authors))
            {
                sw.WriteLine("Authors: " + mspStorage.Authors);
            }
            if (!string.IsNullOrEmpty(mspStorage.CompoundClass))
            {
                sw.WriteLine("COMPOUNDCLASS: " + mspStorage.CompoundClass);
            }

            //sw.WriteLine("License: " + mspStorage.License);
            if (!string.IsNullOrEmpty(mspStorage.Retentiontime))
            {
                sw.WriteLine("RETENTIONTIME: " + mspStorage.Retentiontime);
            }
            if (!string.IsNullOrEmpty(mspStorage.CollisionCrossSection))
            {
                sw.WriteLine("CCS: " + mspStorage.CollisionCrossSection);
            }
            //sw.WriteLine("CHARGE: " + mspStorage.Charge);
            //sw.WriteLine("MASSBANKACCESSION: " + mspStorage.MassBankAccession);
            //sw.WriteLine("Links: " + mspStorage.Links);
            if (!string.IsNullOrEmpty(mspStorage.Ontology))
            {
                sw.WriteLine("ONTOLOGY: " + mspStorage.Ontology);
            }
            sw.WriteLine("COMMENT: " + mspStorage.Comment);

            sw.WriteLine("Num Peaks: " + mspStorage.Peaknum);
            foreach (var peak in mspStorage.Peaks)
            {
                sw.WriteLine(peak.Mz + "\t" + peak.Intensity);
            }
            sw.WriteLine();
        }


        public static void MspFieldsToList(MspStorage mspStorage, List<string> list)
        {
            list.Add("NAME: " + mspStorage.Name);
            list.Add("PRECURSORMZ: " + mspStorage.PrecursorMz);
            list.Add("PRECURSORTYPE: " + mspStorage.PrecursorType);
            list.Add("IONMODE: " + mspStorage.Ionmode);
            if (!string.IsNullOrEmpty(mspStorage.Formula))
            {
                list.Add("FORMULA: " + mspStorage.Formula);
            }
            if (!string.IsNullOrEmpty(mspStorage.Smiles))
            {
                list.Add("SMILES: " + mspStorage.Smiles);
            }
            //list.Add("INCHI: " + mspStorage.Inchi);
            if (!string.IsNullOrEmpty(mspStorage.InchiKey))
            {
                list.Add("INCHIKEY: " + mspStorage.InchiKey);
            }
            if (!string.IsNullOrEmpty(mspStorage.Ionization))
            {
                list.Add("IONIZATION: " + mspStorage.Ionization);
            }
            if (!string.IsNullOrEmpty(mspStorage.InstrumentType))
            {
                list.Add("INSTRUMENTTYPE: " + mspStorage.InstrumentType);
            }
            if (!string.IsNullOrEmpty(mspStorage.Instrument))
            {
                list.Add("INSTRUMENT: " + mspStorage.Instrument);
            }
            if (!string.IsNullOrEmpty(mspStorage.CollisionEnergy))
            {
                list.Add("COLLISIONENERGY: " + mspStorage.CollisionEnergy);
            }
            if (!string.IsNullOrEmpty(mspStorage.Authors))
            {
                list.Add("Authors: " + mspStorage.Authors);
            }
            if (!string.IsNullOrEmpty(mspStorage.CompoundClass))
            {
                list.Add("COMPOUNDCLASS: " + mspStorage.CompoundClass);
            }

            //list.Add("License: " + mspStorage.License);
            if (!string.IsNullOrEmpty(mspStorage.Retentiontime))
            {
                list.Add("RETENTIONTIME: " + mspStorage.Retentiontime);
            }
            if (!string.IsNullOrEmpty(mspStorage.CollisionCrossSection))
            {
                list.Add("CCS: " + mspStorage.CollisionCrossSection);
            }
            //list.Add("CHARGE: " + mspStorage.Charge);
            //list.Add("MASSBANKACCESSION: " + mspStorage.MassBankAccession);
            //list.Add("Links: " + mspStorage.Links);
            if (!string.IsNullOrEmpty(mspStorage.Ontology))
            {
                list.Add("ONTOLOGY: " + mspStorage.Ontology);
            }
            list.Add("COMMENT: " + mspStorage.Comment);

            list.Add("Num Peaks: " + mspStorage.Peaknum);
            foreach (var peak in mspStorage.Peaks)
            {
                list.Add(peak.Mz + "\t" + peak.Intensity);
            }
            list.Add("");
        }

    }


    [DataContract]
    public class MonaJsonRoot
    {
        [DataMember]
        public MonaJson[]? Property1 { get; set; }
    }
    [DataContract]
    public class MonaJson
    {
        [DataMember]
        public Compound[]? compound { get; set; }
        [DataMember]
        public string? id { get; set; }
        [DataMember]
        public long dateCreated { get; set; }
        [DataMember]
        public long lastUpdated { get; set; }
        [DataMember]
        public long lastCurated { get; set; }
        [DataMember]
        public Metadata1[]? metaData { get; set; }
        [DataMember]
        public Score? score { get; set; }
        [DataMember]
        public string? spectrum { get; set; }
        [DataMember]
        public Splash? splash { get; set; }
        [DataMember]
        public Submitter? submitter { get; set; }
        [DataMember]
        public Tag1[]? tags { get; set; }
        [DataMember]
        public Library? library { get; set; }
    }
    [DataContract]
    public class Score
    {
        [DataMember]
        public Impact[]? impacts { get; set; }
        [DataMember]
        public float score { get; set; }
    }
    [DataContract]
    public class Impact
    {
        [DataMember]
        public float value { get; set; }
        [DataMember]
        public string? reason { get; set; }
    }
    [DataContract]
    public class Splash
    {
        [DataMember]
        public string? block1 { get; set; }
        [DataMember]
        public string? block2 { get; set; }
        [DataMember]
        public string? block3 { get; set; }
        [DataMember]
        public string? block4 { get; set; }
        [DataMember]
        public string? splash { get; set; }
    }
    [DataContract]
    public class Submitter
    {
        [DataMember]
        public string? id { get; set; }
        [DataMember]
        public string? emailAddress { get; set; }
        [DataMember]
        public string? firstName { get; set; }
        [DataMember]
        public string? lastName { get; set; }
        [DataMember]
        public string? institution { get; set; }
    }
    [DataContract]
    public class Library
    {
        [DataMember]
        public string? library { get; set; }
        [DataMember]
        public string? description { get; set; }
        [DataMember]
        public string? link { get; set; }
        [DataMember]
        public Tag? tag { get; set; }
    }
    [DataContract]
    public class Tag
    {
        [DataMember]
        public bool ruleBased { get; set; }
        [DataMember]
        public string? text { get; set; }
    }
    [DataContract]
    public class Compound
    {
        [DataMember]
        public string? inchi { get; set; }
        [DataMember]
        public string? inchiKey { get; set; }
        [DataMember]
        public Metadata[]? metaData { get; set; }
        [DataMember]
        public string? molFile { get; set; }
        [DataMember]
        public Name[]? names { get; set; }
        [DataMember]
        public object[]? tags { get; set; }
        [DataMember]
        public bool computed { get; set; }
        [DataMember]
        public string? kind { get; set; }
        [DataMember]
        public Classification[]? classification { get; set; }
    }
    [DataContract]
    public class Metadata
    {
        [DataMember]
        public string? category { get; set; }
        [DataMember]
        public bool computed { get; set; }
        [DataMember]
        public bool hidden { get; set; }
        [DataMember]
        public string? name { get; set; }
        [DataMember]
        public string? value { get; set; }
    }
    [DataContract]
    public class Name
    {
        [DataMember]
        public bool computed { get; set; }
        [DataMember]
        public string? name { get; set; }
        [DataMember]
        public float score { get; set; }
    }
    [DataContract]
    public class Classification
    {
        [DataMember]
        public string? category { get; set; }
        [DataMember]
        public bool computed { get; set; }
        [DataMember]
        public bool hidden { get; set; }
        [DataMember]
        public string? name { get; set; }
        [DataMember]
        public string? value { get; set; }
    }

    public class Metadata1
    {
        [DataMember]
        public string? category { get; set; }
        [DataMember]
        public bool computed { get; set; }
        [DataMember]
        public bool hidden { get; set; }
        [DataMember]
        public string? name { get; set; }
        [DataMember]
        public string? value { get; set; }
        [DataMember]
        public string? unit { get; set; }
    }

    public class Tag1
    {
        [DataMember]
        public bool ruleBased { get; set; }
        [DataMember]
        public string? text { get; set; }
    }
    public class MspStorage
    {
        private string? name;
        private string? precursorMz;
        private string? precursorType;
        private string? instrumentType;
        private string? instrument;
        private string? authors;
        private string? license;
        private string? smiles;
        private string? tmsSmiles;

        private string? inchi;
        private string? inchiKey;
        private string? basepeakIntensity;
        private string? comment;
        private string? collisionEnergy;
        private string? formula;
        private string? retentiontime;
        private string? retentionIndex;
        private string? compoundClass;
        private string? id;
        private string? challengename;
        private string? xlogP;
        private string? sssCH2;
        private string? meanI;
        private string? mslevel;
        private string? exactmass;
        private string? binID;
        private string? quantMass;
        private string? ontology;
        private string? ontologyID;

        private string? ionmode;
        private string? ionization;
        private string? charge;
        private string? links;
        private string? massBankAccession;
        private string? peaknum;
        private string? cas;

        private string? collisionCrossSection;


        private List<MspPeak> peaks;

        public MspStorage()
        {
            peaks = new List<MspPeak>();
        }

        public string? Name
        { get; set; }
        public string? PrecursorMz
        { get; set; }
        public string? PrecursorType
        { get; set; }
        public string? Exactmass
        { get; set; }
        public string? Mslevel
        { get; set; }
        public string? Id
        { get; set; }
        public string? BinID
        { get; set; }
        public string? Challengename
        { get; set; }
        public string? InstrumentType
        { get; set; }
        public string? Instrument
        { get; set; }
        public string? Authors
        { get; set; }
        public string? QuantMass
        { get; set; }
        public string? License
        { get; set; }
        public string? Smiles
        { get; set; }
        public string? TmsSmiles
        { get; set; }
        public string? Inchi
        { get; set; }
        public string? Comment
        { get; set; }
        public string? RetentionIndex
        { get; set; }
        public string? CollisionEnergy
        { get; set; }
        public string? Formula
        { get; set; }
        public string? Retentiontime
        { get; set; }
        public string? MassBankAccession
        { get; set; }
        public string? Charge
        { get; set; }
        public string? Ionmode
        { get; set; }
        public string? Ionization
        { get; set; }
        public string? Links
        { get; set; }
        public string? Peaknum
        {
            get { return peaknum; }
            set { peaknum = value; }
        }

        public List<MspPeak> Peaks
        {
            get { return peaks; }
            set { peaks = value; }
        }

        public string? InchiKey
        { get; set; }
        public string? CompoundClass
        { get; set; }
        public string? XlogP
        { get; set; }
        public string? SssCH2
        { get; set; }
        public string? MeanI
        { get; set; }
        public string? Cas
        { get; set; }
        public string? Ontology
        { get; set; }
        public string? OntologyID
        { get; set; }
        public string? BasepeakIntensity
        { get; set; }
        public string? CollisionCrossSection
        { get; set; }
    }
    public class MspPeak
    {
        double mz;
        double intensity;
        string? comment;
        string? frag;

        public double Mz
        {
            get { return mz; }
            set { mz = value; }
        }

        public double Intensity
        {
            get { return intensity; }
            set { intensity = value; }
        }

        public string? Comment
        { get; set; }
        public string? Frag
        { get; set; }
    }

}
