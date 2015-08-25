using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq; // for Where
using System.Collections.Generic;

public class CheckButtonControl : MonoBehaviour {

	public Text resText;
	public const string kTitle = "<title>";
	public InputField IFtelno;
	public InputField IFinfo;

	private Dictionary<string,string> telbook = new Dictionary<string, string>();

	void Start() {
		IFtelno.text = "0729883121"; // TODO: remove
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
		//      string telno = "0729883121"; // registered
		//      string telno = "0729883120"; // not registered
		string telno = IFtelno.text;
		string url = "http://www.jpcaller.com/phone/";
		
		// remove "-" from telno
		telno = removeHyphen (telno);
		
		WWW web = new WWW(url + telno);
		yield return web;
		
		string res = web.text;
		int pos = res.IndexOf (kTitle);
		resText.text = "not found";
		IFinfo.text = "";
		if (pos > 0) {
			res = getHospitalName(web.text.Substring(pos, 40), telno);
			resText.text = res;
			if (hasObtainedTelNo(res)) {
				IFinfo.text = resText.text;
				addDictionary(telno, resText.text);
			}
		}
	}
	
	void addDictionary(string telno, string name) {
		if (telbook.ContainsKey (telno) == false) {
			telbook.Add (telno, name);
			Debug.Log("added");
		}
//		var res = telbook ["012345678"];
//		Debug.Log (res);
	}

	public void CheckButtonOnClick() {
//		testDic ();
		StartCoroutine("checkHospitalTelephoneNumber");
	}
}
