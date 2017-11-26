using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MeasuringDeviceAnalizer
{
	/// <summary>
	/// Description of ModelItemsToSerialize.
	/// </summary>
	[Serializable()]
	public class ModelItemsToSerialize : ISerializable
	{	
		public RowClass[] RowArray;
        public List<ModelVarClass> ModelVars;  
        public List<ModelVarClass> ErrorVars;  
        public string[] Results;
        public string[] SubstitutionResults;				
        public string BlockSchemePath;
        public string SignalOut;
        public string SignalIn;
		
		public ModelItemsToSerialize() 
		{
		}	

   		public ModelItemsToSerialize(SerializationInfo info, StreamingContext ctxt)
   		{
            try
            {
                this.RowArray = (RowClass[])info.GetValue("RowArray", typeof(RowClass[]));
            }
            catch (Exception e) { }
            try
            {
                this.ModelVars = (List<ModelVarClass>)info.GetValue("ModelVars", typeof(List<ModelVarClass>));
            }
            catch (Exception e) { }
            try
            {
                this.ErrorVars = (List<ModelVarClass>)info.GetValue("ErrorVars", typeof(List<ModelVarClass>));
            }
            catch (Exception e) { }
            try
            {
       	  		this.Results = (string[])info.GetValue("Results", typeof(string[]));
            }
            catch (Exception e) { }
            try
            {
   	      		this.SubstitutionResults = (string[])info.GetValue("SubstitutionResults", typeof(string[])); 
            }    
            catch (Exception e) { }
            try
            {
		    	this.BlockSchemePath = (string)info.GetValue("BlockSchemePath", typeof(string));
            }
            catch (Exception e) { }
            try
            {
                this.SignalOut = (string)info.GetValue("SignalOut", typeof(string));
            }
            catch (Exception e) { }
            try
            {
                this.SignalIn = (string)info.GetValue("SignalIn", typeof(string));
            }
            catch (Exception e) { }
   		}
   		
   		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   		{
      		info.AddValue("RowArray", this.RowArray);
      		info.AddValue("ModelVars", this.ModelVars);
      		info.AddValue("ErrorVars", this.ErrorVars);
      		info.AddValue("Results", this.Results);
      		info.AddValue("SubstitutionResults", this.SubstitutionResults);
			info.AddValue("BlockSchemePath", this.BlockSchemePath);
            info.AddValue("SignalOut", this.SignalOut);
            info.AddValue("SignalIn", this.SignalIn);
   		} 	
   		
   		public void Clear()
   		{
			RowArray = null;
    	   	ModelVars = null; 
			ErrorVars = null;    	   	
    	   	Results = null;
    	    SubstitutionResults = null;	
			BlockSchemePath = null;
            SignalOut = null;
            SignalIn = null;
   		}
	}
}
