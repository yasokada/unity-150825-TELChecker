using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq; // for Where
using System.Collections.Generic;
using System; // for StringSplitOptions.RemoveEmptyEntries

public class CheckButtonControl : MonoBehaviour {

	public const string kTitle = "<title>";
	public Text resText; // to show check result
	public InputField IFtelno; // for input telephone number
	public InputField IFinfo; // to show/input hospital name

	public const string kDicFile = "telbook.txt";
	static private Dictionary<string,string> telbook = new Dictionary<string, string>();
	private bool justStarted = true;
	
	void Start() {
		IFtelno.text = "0729883121"; // TODO: remove // for test
	}

	string getHospitalName(string txt, string telno) {
		string removed = txt.Substring (kTitle.Length, 30);
		
		// check with first 2 characters
		string lhs = removed.Substring (0, 2);
		string rhs = telno.Substring (0, 2);
		if (lhs.Equals (rhs)) {
			return "not registered";
		}
		return removed;
	}
	
	string removeHyphen(string src) {
		var dst = new string (src.Where (c => !"-".Contains (c)).ToArray ());
		return dst;
	}

	bool hasObtainedTelNo(string src) {
		if (src.ToLower ().Contains ("not")) {
			return false;
		}
		return true;
	}

	IEnumerator checkHospitalTelephoneNumber() {
		string telno = IFtelno.text;

		// remove "-" from telno
		telno = removeHyphen (telno);

		if (telbook.ContainsKey (telno)) {
			IFinfo.text = "dic:" + telbook[telno];
			yield break;
		}
		
		string url = "http://www.jpcaller.com/phone/";
		WWW web = new WWW(url + telno);
		yield return web;
		
		string res = web.text;
		int pos = res.IndexOf (kTitle);
		resText.text = "not found";
		IFinfo.text = "";
		if (pos > 0) {
			res = getHospitalName(web.text.Substring(pos, 40), telno);
			resText.text = extractCsvRow(res, 0, /* spaceDiv=*/true);
			if (hasObtainedTelNo(res)) {
				IFinfo.text = resText.text;
			}
		}
	}
	
	void addDictionary(string telno, string name) {
		if (telbook.ContainsKey (telno) == false) {
			telbook.Add (telno, name);
			Debug.Log("added");
		}
	}
	
	public void CheckButtonOnClick() {
		if (justStarted) {
			justStarted = false;
			file_import();
		}
		StartCoroutine("checkHospitalTelephoneNumber");
	}

	public void AddButtonOnClick() {
		string telno = removeHyphen (IFtelno.text);
		addDictionary (telno, IFinfo.text);

		file_export (); // TODO: remove // for test
	}

	private string extractCsvRow(string src, int idx, bool spaceDiv)
	{
		string[] splitted = src.Split(new string[] { System.Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
		string res = "";
		foreach(string each in splitted) {
			if (spaceDiv) {
				string [] elements = each.Split(' ');
				res = res + elements[idx] + System.Environment.NewLine;
			} else {
				string [] elements = each.Split(',');
				res = res + elements[idx] + System.Environment.NewLine;
			}
		}
		return res;
	}

	string removeNewLine(string src) {
		if (src.Contains (System.Environment.NewLine)) {
			int pos = src.IndexOf(System.Environment.NewLine);
			return src.Substring(0, pos - 1);
		}
		return src;
	}

	public void file_export()
	{
		string line = "";
		string value;
		foreach (KeyValuePair<string, string> pair in telbook) {
			value = removeNewLine(pair.Value);
			line = line + pair.Key + "," + value + System.Environment.NewLine;
		}
		System.IO.File.WriteAllText (kDicFile, line);
	}

	public void file_import()
	{
		string line = System.IO.File.ReadAllText (kDicFile);

		string[] splitted = line.Split(new string[] { System.Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
		string res = "";
		string key;
		string value;
		foreach(string eachline in splitted) {
			key = extractCsvRow(eachline, 0, /* spaceDiv=*/false);
			value = extractCsvRow(eachline, 1, /* spaceDiv=*/false);
			telbook.Add(key, value);
		}
	}
	
}
