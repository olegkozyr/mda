using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace BlockScheme
{
	/// <summary>
	/// Description of VisualToSerialize.
	/// </summary>
	[Serializable()]
	public class VisualToSerialize : ISerializable
	{	
		private List<ShapeBase> shapeBase = new List<ShapeBase>();
		
		public VisualToSerialize() 
		{
		}

   		public VisualToSerialize(SerializationInfo info, StreamingContext ctxt)
   		{
      		this.shapeBase = (List<ShapeBase>)info.GetValue("ShapeBase", typeof(List<ShapeBase>));
   		}

   		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   		{
      		info.AddValue("ShapeBase", this.shapeBase);
   		} 		
		
		public List<ShapeBase> GetShapeBases()
		{
			return shapeBase;
		}
		
		public void SetShapeBase(List<Visual> visuals)
		{
			int count = visuals.Count;
			for (int i = 0; i < count; i++)
			{
				shapeBase.Add(visuals[i] as ShapeBase);
			}
		}

		public int GetCount()
		{
			return shapeBase.Count;
		}				
	}
}
