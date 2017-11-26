using System;
using System.Windows;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32;
using System.Reflection;
using BlockScheme;

namespace MeasuringDeviceAnalizer
{
	/// <summary>
	/// Выполняет все операции по сохранению и открытию файлов.
	/// </summary>
	public static class OpenSaveClass
	{
		static OpenSaveClass()
		{
		}		

		static public bool SaveCommand(string filePathFrom, string filePathTo, out string exeption)
		{
			exeption = string.Empty;
			if (string.IsNullOrEmpty(filePathFrom) && string.IsNullOrEmpty(filePathTo))
			{
				return false;
			}
			
	        try
	        {
		    	File.Copy(filePathFrom, filePathTo, true);
	        }
        	catch (Exception e)
        	{
        		exeption = e.Message;
        		return false;
        	}

			return true;        	
		}
		
        static public bool SaveCommand(string fileName, object objectToSerialize, out string exeption)
        {
        	exeption = string.Empty;

           	if(objectToSerialize == null)
           	{
           		return false;
           	} 
           	
			Stream stream;
      		try
      		{			
				stream = File.Open(fileName, FileMode.Create);
      		}
      		catch (Exception e)
      		{
      			exeption = e.Message;
      			return false;
      		}
      		BinaryFormatter bFormatter = new BinaryFormatter();
      		try
      		{      		
      			bFormatter.Serialize(stream, objectToSerialize);
      		}
      		catch (Exception e)
      		{
      			exeption = e.Message;
      			return false;
      		}
      		
      		stream.Close();  
			return true;      		
        }

        static public bool SaveCommand(object objectToSerialize, out string filePath, out string exeption)
        {
        	filePath = exeption = string.Empty;
           	if(objectToSerialize == null)
           	{
           		return false;
           	} 

			filePath = System.IO.Path.Combine(CurrentDir(DirName.Temp) + @"\temp" + ".bs"); 
           	
			if (SaveCommand(filePath, objectToSerialize, out exeption))
			{
				return true;
			}
			else
			{
				return false;
			}
        }
        
        static public object OpenCommand(string fileName, ModelType model, out string exeption)
        {
        	exeption = string.Empty;
           	if (!File.Exists(fileName))
           	{
           		exeption = "Файл не існує";
           		return null;
           	}
           	
			Stream stream;
      		try
      		{
	      		stream = File.Open(fileName, FileMode.Open);
      		}
      		catch (Exception e)
      		{ 
				exeption = e.Message; 			
      			return null;
      		}
      		BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Binder = new AllAssemblyVersionsDeserializationBinder();
      		object obj;
      		try
      		{
      			obj = bFormatter.Deserialize(stream);
      		}
      		catch (Exception e)
      		{
				exeption = e.Message;
                stream.Close();
      			return null;
      		}
      		stream.Close();
      		if ((model == ModelType.Matrix) && ((obj is ModelItemsToSerialize)))
      		{
      			return obj;
      		}
            else if ((model == ModelType.Scheme) && ((obj is VisualToSerialize)))
            {
                return obj;
            }

			exeption = "Невірний формат файлу";      		
			return null;
        }		
		
		static private string CurrentDir(DirName dirName)
		{
			string path;
			if (dirName == DirName.Library)
				path = "library";
			else
				path = "temp";
			
            DirectoryInfo dir = new DirectoryInfo(
				System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\" + path);
            if (!dir.Exists)
            {
            	dir.Create();
            }            
			return dir.FullName;			
		}		
		
        static public object OpenDialog(ModelType model, out string exeption)
        { 
        	exeption = string.Empty;
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.InitialDirectory = CurrentDir(DirName.Library);
            if (model == ModelType.Matrix)
            {
	            openDlg.Filter = "Матрична Модель (*.md)|*.md";                	
            }
            else if (model == ModelType.Scheme)
            {
    	        openDlg.Filter = "Структурна схема (*.bs)|*.bs";
            }			
			openDlg.Filter += "|Всі файли (*.*)|*.*";
            
            if (true == openDlg.ShowDialog())
            {    	   		
    	   		return OpenCommand(openDlg.FileName, model, out exeption);        		
            }
            else
            {
            	return null;
            }
        }
        
        static public string SaveDialog(ModelType model, string filePath)
        {       			
            SaveFileDialog saveDlg = new SaveFileDialog();
            if (model == ModelType.Matrix)
            {
    	        saveDlg.FileName = "MatModel";
	            saveDlg.Filter = "Матрична Модель (*.md)|*.md";                	
            }
            else if (model == ModelType.Scheme)
            {
	            saveDlg.FileName = "Scheme";
    	        saveDlg.Filter = "Структурна схема (*.bs)|*.bs";
            }            
            saveDlg.Filter += "|Всі файли (*.*)|*.*"; 
            string directoryPath = string.Empty;
            if ((!string.IsNullOrEmpty(filePath)) && (File.Exists(filePath)))
            {
            	try
            	{
            		directoryPath = Path.GetDirectoryName(filePath);
            	}
            	catch {}
            	if (string.IsNullOrEmpty(directoryPath))
            	{
            		saveDlg.InitialDirectory = CurrentDir(DirName.Library);
            	}
            	else
            	{
            		saveDlg.InitialDirectory = directoryPath;
            	}
            }
            else
            {
            	saveDlg.InitialDirectory = CurrentDir(DirName.Library);
            }
            if (true == saveDlg.ShowDialog())
            {
            	if ((model == ModelType.Matrix) && (!saveDlg.FileName.EndsWith(".md")))
            	{
	            	saveDlg.FileName = saveDlg.FileName + ".md";    	        	            	            	
            	}
            	else if ((model == ModelType.Scheme) && (!saveDlg.FileName.EndsWith(".bs")))
            	{
	            	saveDlg.FileName = saveDlg.FileName + ".bs";    	        	             		
            	}
            	            	              	
            	return saveDlg.FileName;          	
            }
            else
            {
            	return string.Empty;
            }
        } 		
	}

    sealed class AllAssemblyVersionsDeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string loadAssemName, string typeName)
        {
            Type modelType = null;
            String currentAssemName = Assembly.GetExecutingAssembly().FullName;

            if (loadAssemName.Contains("mscorlib"))
            {
                if (typeName.Contains("mscorlib") || typeName.Contains("WindowsBase"))
                {
                    modelType = Type.GetType(String.Format("{0}, {1}",
                        typeName, loadAssemName));
                }
                else
                {
                    int indexTypeName = typeName.IndexOf(",");
                    string temp = typeName.Substring(0, indexTypeName) + ", " + currentAssemName + "]]";
                    modelType = Type.GetType(String.Format("{0}, {1}",
                        temp, loadAssemName));
                }
            }
            else if (loadAssemName.Contains("WindowsBase"))
            {
                modelType = Type.GetType(String.Format("{0}, {1}",
                    typeName, loadAssemName));
            }
            else
            {
                modelType = Type.GetType(String.Format("{0}, {1}",
                    typeName, currentAssemName));
            }
            return modelType;
        }
    }

	public enum ModelType
	{
		Matrix,
		Scheme,
	}
	
	public enum DirName
	{
		Library,
		Temp,
	}
}
