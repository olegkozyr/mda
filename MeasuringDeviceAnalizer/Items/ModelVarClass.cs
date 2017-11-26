using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MeasuringDeviceAnalizer
{
	/// <summary>
	/// Description of ModelVarClass.
	/// </summary>
	[Serializable()]
	public class ModelVarClass
	{
		public int VarNum  {set; get;} //для номеров рядков в таблицах
		public string Variable {set; get;}
		public string VarValue {set; get;}
		public bool? IsUsed {set; get;}		
		
		public ModelVarClass()
		{
		}
		
		public ModelVarClass(int varNum, string variable, bool? isUsed)
		{
			VarNum = varNum;
			Variable = variable;
			IsUsed = isUsed;
		}

		public ModelVarClass(SerializationInfo info, StreamingContext ctxt)
		{
			this.VarNum = (int)info.GetValue("VarNum", typeof(int));
			this.Variable = (string)info.GetValue("Variable", typeof(string));
			this.VarValue = (string)info.GetValue("VarValue", typeof(string));
			this.IsUsed = (bool?)info.GetValue("IsUsed", typeof(bool?));
		}		

   		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   		{
      		info.AddValue("VarNum", this.VarNum);
      		info.AddValue("Variable", this.Variable);
      		info.AddValue("VarValue", this.VarValue);
      		info.AddValue("IsUsed", this.IsUsed);
   		} 		
	}
}