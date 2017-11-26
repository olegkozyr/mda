using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MeasuringDeviceAnalizer
{
	/// <summary>
	/// Description of RowClass.
	/// </summary>
	[Serializable()]
	public class RowClass
	{
		public int RowNum {set; get;}
        public string[] Row { set; get; }        
        public string RowX { set; get; }
        public string RowB { set; get; }

        public RowClass(){}
        public RowClass(int columnsCount, int index)
		{
        	RowNum = index;
            Row = new string[columnsCount];
            RowX = "x" + index;
		}
        
		public RowClass(SerializationInfo info, StreamingContext ctxt)
		{
			this.RowNum = (int)info.GetValue("RowNum", typeof(int));
			this.Row = (string[])info.GetValue("Row", typeof(string[]));
			this.RowX = (string)info.GetValue("RowX", typeof(string));
			this.RowB = (string)info.GetValue("RowB", typeof(string));
		}		

   		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   		{
      		info.AddValue("RowNum", this.RowNum);
      		info.AddValue("Row", this.Row);
      		info.AddValue("RowX", this.RowX);
      		info.AddValue("RowB", this.RowB);
   		}      
	}
}
