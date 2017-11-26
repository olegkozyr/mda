using System;
using System.Windows.Media;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace BlockScheme
{
	/// <summary>
	/// Description of ShapeBase.
	/// </summary>
	[Serializable()]
	public abstract class ShapeBase : DrawingVisual, ISerializable 
	{		
		public ShapeBase()
		{
		}
		
		public abstract void SelfDisposal();
		public abstract void ClearConnection( int num );
		public abstract void GetObjectData(SerializationInfo info, StreamingContext ctxt);
		public abstract bool IsConnected();
	}
}
