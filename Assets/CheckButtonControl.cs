using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq; // for Where
using System.Collections.Generic;
using System; // for StringSplitOptions.RemoveEmptyEntries

/*
 * V0.2 
 *   exec file_import() on Start() not on CheckButtonOnClick()
 * V0.1 2015/08/26
 *   add dictionary export/import feature
 *   add web search / dictionary search feature
 */

public class CheckButtonControl : MonoBehaviour {

	public const string kTitle = "<title>";
	public const string kVersion = "TELChecker V0.2";

	public Text resText; // to show check result
	public Text verText; // to show version info 
	public InputField IFtelno; // for input telephone number
	public InputField IFinfo; // to show/input hospital name

	public const string kDicFile = "telbook.txt";
	static private Dictionary<string,string> telbook = new Dictionary<string, string>();
	static private bool justStarted = true;
	
	void Start() {
		IFtelno.text = "072-988-3121"; // TODO: remove // for test

		if (justStarted) {
			justStarted = false;
			file_import();
			verText.text = kVersion;
		}
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
			resText.text = "found on the dictionary";
			IFinfo.text = telbook[telno];
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
			if (hasObtainedTelNo(res)) {
				resText.text = extractCsvRow(res, 0, /* spaceDiv=*/true);
				IFinfo.text = resText.text;
			} else {
				resText.text = res;
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
		StartCoroutine("checkHospitalTelephoneNumber");
	}

	public void AddButtonOnClick() {
		string telno = removeHyphen (IFtelno.text);
		addDictionary (telno, IFinfo.text);

		file_export ();
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
			if (src.Length <= 1) {
				return "";
			}
			int pos = src.IndexOf(System.Environment.NewLine);
			return src.Substring(0, pos);
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
		string key;
		string value;
		foreach(string eachline in splitted) {
			key = extractCsvRow(eachline, 0, /* spaceDiv=*/false);
			value = extractCsvRow(eachline, 1, /* spaceDiv=*/false);

			key = removeNewLine(key);
			value = removeNewLine(value);

			if (value.Length == 0) {
				continue;
			}
			telbook.Add(key, value);
		}
	}
	
}
