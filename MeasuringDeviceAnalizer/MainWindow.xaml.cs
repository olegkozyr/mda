#region using
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using BlockScheme;
using System.Data.Common;
using System.Data;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Win32;
#endregion

namespace MeasuringDeviceAnalizer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region filds
		private static readonly int MinusOne = -1;
		public static readonly int InitialArraySize = 3;
		private static readonly int ResCount = 6;
        private readonly string[] ResultsHeaders = new string[]
        {
        	"Визначник системи:",
        	"Повний коефіцієнт передачі:",
        	"Коефіцієнт передачі:",
			"det(C)ф=",
			"det(C)ч=",
			"kф",
			"kч",
			"delta",
			"порядок",
        };				
        private static readonly char[] operators = new char[]
        {
        	'(', ')', '^', '*', '/', '-', '+',
        };
        
		#region basic variables
		#region serializable
        private RowClass[] RowArray;
        private List<ModelVarClass> ModelVars;     
 		private List<ModelVarClass> ErrorVars;         
        //Результаты решения и результаты с подстановкой значений        
        private string[] Results = new string[ResCount];
        private string[] SubstitutionResults = new string[ResCount];
		//путь к файлу блок-схемы
		public string BlockSchemePath = string.Empty;
        private string SignalOut = string.Empty;
        private string SignalIn = string.Empty;
		#endregion
		private int ErrorOrder = MinusOne;		
		#endregion       
			
		//переменная окна редактора блок-схем
        private BlockSchemeWindow blockSchemeWindow;  

		//контейнеры
		bool?[] IsUsed = new bool?[2]{false, false};

		//технические переменные
        private DataGridTextColumn column;
        private Regex isSymbolic = new Regex(@"[^\d\*/\-\+\^\.\,\(\)]", RegexOptions.Compiled);
		#endregion
        
		#region constructors 		
		public MainWindow()
		{
			InitializeComponent();
			  
			ModelVars = new List<ModelVarClass>(); 
			ErrorVars = new List<ModelVarClass>();			
			SetArrays(InitialArraySize);		
            StateSwitcher(false);                        
		}
		#endregion
   		
		#region methods 
		#region files & state operations		
		public void StateSwitcher(bool onBlockShemeMode)
		{
			if (!onBlockShemeMode)
			{
				BlockSchemePath = string.Empty;
			}
			dataTable.IsReadOnly = onBlockShemeMode;
			dataTableXB.Columns[1].IsReadOnly = onBlockShemeMode;
			EditModel.IsEnabled = onBlockShemeMode;
			setArraySizeTextBox.IsEnabled = !onBlockShemeMode;
			setArraySizeBtn.IsEnabled = !onBlockShemeMode;	
		}	

		public void InitModel(bool isInitAnew)
		{
			StateSwitcher(false);
			
			if (blockSchemeWindow != null)
			{
				blockSchemeWindow.WindowDisposal();
				blockSchemeWindow = null;
			}								
			
			if (isInitAnew)
			{        	
 				SetArrays(InitialArraySize);
 				ButtonsReset();
			}			
 			ModelVarSubst.ItemsSource = null;
 			ErrorVarSubst.ItemsSource = null;
 			ModelVars.Clear();
 			ErrorVars.Clear();
 			
 			determinantTextBox.Clear();
 			substitutionTextBox.Clear();
			for (int i = 0; i < ResCount; i++)
			{
				Results[i] = string.Empty;
				SubstitutionResults[i] = string.Empty;
			}
            SignalOut = string.Empty;
            SignalIn = string.Empty;
            CompleteTransRatio.IsChecked = false;
            TransRatioOutTxtBox.Text = string.Empty;
            TransRatioInTxtBox.Text = string.Empty;
		}

		public void ButtonsReset()
		{
			CompleteTransRatio.IsChecked = false;
			TransRatioOutTxtBox.Text = string.Empty;
			TransRatioInTxtBox.Text = string.Empty;
			SystemDeterminant.IsChecked = false;
			DetermErrorModel.IsChecked = false;
			ErrorOrder = MinusOne;	
			VarSubst.SelectedIndex = 0;			
		}
		#endregion
		
		#region initialization
		public void SetArrays(string[,] array, string[] arrayB)
		{
			int arraySize = array.GetLength(0), count, i, j;
            count = dataTable.Columns.Count;			
            
            if (arraySize == count)
            {
            	for (i = 0; i < arraySize; i++)
            	{
            		for (j = 0; j < arraySize; j++)
            		{
            			RowArray[i].Row[j] = array[i, j];
            		}
            		
            		RowArray[i].RowB = "0";
            	}
             	
            	for (i = 0; i < arrayB.Length; i++)
            	{
            		RowArray[i].RowB = arrayB[i];
            	}
            	dataTable.Items.Refresh();
            	dataTableXB.Items.Refresh();            	
            	return;
            }
            
            dataTable.ItemsSource = null;
            dataTableXB.ItemsSource = null;
            RowArray = new RowClass[arraySize];
            for (i = 0; i < arraySize; i++)
            {
                RowArray[i] = new RowClass(arraySize, i + 1);
            }

            for (i = 0; i < arraySize; i++)
            {
            	for (j = 0; j < arraySize; j++)
            	{
            		RowArray[i].Row[j] = array[i, j];
            	}
            		
            	RowArray[i].RowB = "0";
            }            

            for (i = 0; i < arrayB.Length; i++)
            {
            	RowArray[i].RowB = arrayB[i];
            }
            
            if (arraySize > count)
            {
                for (i = count; i < arraySize; i++)
                {
                    column = new DataGridTextColumn();
                    column.Header = Convert.ToString(i + 1);
                    column.Binding = new Binding("Row[" + i + "]");
                    dataTable.Columns.Add(column);                                        
                }
            }
            else
            {
                for (i = count - 1; i >= arraySize; i--)
                {
                    dataTable.Columns.RemoveAt(i);
                }
            }

            dataTable.ItemsSource = RowArray;
			dataTableXB.ItemsSource = RowArray; 			
		}										

		private void SetArrays(int arraySize)
		{			
			int i, j, count = dataTable.Columns.Count;		
            
            if (arraySize == count)
            {
            	for (i = 0; i < arraySize; i++)
            	{
            		for (j = 0; j < arraySize; j++)
            		{
            			RowArray[i].Row[j] = "0";
            		}
            		
            		RowArray[i].RowB = "0";
            	}
            	
            	dataTable.Items.Refresh();
            	dataTableXB.Items.Refresh();
            	
            	return;
            }
            
            dataTable.ItemsSource = null;
            dataTableXB.ItemsSource = null;
            RowArray = new RowClass[arraySize];
            for (i = 0; i < arraySize; i++)
            {
                RowArray[i] = new RowClass(arraySize, i + 1);
            }

           	for (i = 0; i < arraySize; i++)
           	{
           		for (j = 0; j < arraySize; j++)
           		{
           			RowArray[i].Row[j] = "0";
           		}
           		
           		RowArray[i].RowB = "0";
           	}            
            
            if (arraySize > count)
            {
                for (i = count; i < arraySize; i++)
                {
                    column = new DataGridTextColumn();
                    column.Header = Convert.ToString(i + 1);
                    column.Binding = new Binding("Row[" + i + "]");
                    dataTable.Columns.Add(column);                                        
                }
            }
            else
            {
                for (i = count - 1; i >= arraySize; i--)
                {
                    dataTable.Columns.RemoveAt(i);
                }
            }

            dataTable.ItemsSource = RowArray;
			dataTableXB.ItemsSource = RowArray; 			
		}
		
		private void ModelVarFill(string str)
		{
			int t;
			
			if (str.StartsWith("-"))
				str = str.Substring(1);
						
			for (t = 0; (t < ModelVars.Count) && (!ModelVars[t].Variable.Equals(str)); 
				t++) {}
			if (t >= ModelVars.Count)
			{
                ModelVars.Add(new ModelVarClass(ModelVars.Count + 1,
                    str, SelectAllChechBox.IsChecked));	
			}
		}		
		#endregion		

		#region expression evaluation	
        private string ValueSubstitution(string exp)
        {
            int k, minus, varsCount;
            string tempCell, cell;
            bool isMatch;

            cell = exp;
            minus = 1;
            if (cell.StartsWith("-"))
            {
                cell = cell.Substring(1);
                minus = -1;
            }

            varsCount = ModelVars.Count;
            isMatch = false;
            for (k = 0; (k < varsCount) && !isMatch; k++)
            {
                if ((ModelVars[k].IsUsed == true) && 
                    (isMatch = cell.Equals(ModelVars[k].Variable)))
                {
                    tempCell = ModelVars[k].VarValue;
                    if (tempCell.Equals("0") || tempCell.Equals("-0"))
                        cell = "0";
                    else if ((minus == -1) && (tempCell.StartsWith("-")))
                        cell = tempCell.Substring(1);
                    else if (minus == -1)
                        cell = "-" + tempCell;
                    else
                        cell = tempCell;
                }
            }

            if (!isMatch)
                cell = exp;

            return cell;
        }

		private string ResultFormation()
		{
			string result, subst;
			result = subst = string.Empty;
			if (!string.IsNullOrEmpty(Results[0]))
			{
				result = ResultsHeaders[0] + "\n" + ResultsHeaders[3] + "\n" + Results[0]; 				
			}				
			if (!string.IsNullOrEmpty(Results[3]))
			{
				result += "\n" + ResultsHeaders[7] + "(C)(" + ResultsHeaders[8] + "=" + 
					ErrorOrder + ")=\n" + Results[3];
			}			
			if (!string.IsNullOrEmpty(Results[1]))
			{
				if (result != string.Empty)
				{
					result += "\n\n";
				}
				result += ResultsHeaders[1] + "\n" + ResultsHeaders[5] + "(" + 
					RowArray.GetLength(0).ToString() + "/1)=\n" + Results[1];				
			}			
			if (!string.IsNullOrEmpty(Results[4]))
			{			
				result += "\n" + ResultsHeaders[7] + "(" + RowArray.GetLength(0) + 
					"/1)(" + ResultsHeaders[8] + "=" + ErrorOrder + ")=\n" + Results[4];					
			}
			
			if (!string.IsNullOrEmpty(Results[2]))
			{                
				if (result != string.Empty)
				{
					result += "\n\n";
				}
                if (!string.IsNullOrEmpty(SignalOut) && !string.IsNullOrEmpty(SignalIn))
				    result += ResultsHeaders[2] + "\n" + ResultsHeaders[5] + "(" +
                        SignalOut + "/" + SignalIn + ")=\n" + Results[2];	
                else
                    result += "x(" + SignalOut + ")ф=\n" +
                        Results[2];	
			}	

			if (!string.IsNullOrEmpty(Results[5]))
			{
                result += "\n" + ResultsHeaders[7] + "(" + SignalOut +
                    "/" + SignalIn + ")(" + ResultsHeaders[8] + 
					"=" + ErrorOrder + ")=\n" + Results[5];				
			}
			
			return result;
		}

		private string SubstResFormation()
		{
			string result, subst;
			result = subst = string.Empty;
			if (!string.IsNullOrEmpty(SubstitutionResults[0]))
			{
				result = ResultsHeaders[0] + "\n" + ResultsHeaders[4] + "\n" + 
					SubstitutionResults[0];
			}
				
			if (!string.IsNullOrEmpty(SubstitutionResults[3]))
			{
				result += "\n" + ResultsHeaders[7] + "(C)(" + ResultsHeaders[8] + "=" + 
					ErrorOrder + ")=\n" + SubstitutionResults[3];
			}
			
			if (!string.IsNullOrEmpty(SubstitutionResults[1]))
			{
				if (result != string.Empty)
				{
					result += "\n\n";
				}						
				result += ResultsHeaders[1] + "\n" + ResultsHeaders[6] + "(" + 
					Convert.ToString(RowArray.Length) + "/1)=\n" + SubstitutionResults[1];			
			}			

			if (!string.IsNullOrEmpty(SubstitutionResults[4]))
			{			
				result += "\n" + ResultsHeaders[7] + "(" + RowArray.GetLength(0) +
					"/1)(" + ResultsHeaders[8] + "=" + ErrorOrder + ")=\n" + 
					SubstitutionResults[4];					
			}
			
			if (!string.IsNullOrEmpty(SubstitutionResults[2]))
			{
				if (result != string.Empty)
				{
					result += "\n\n";
				}
                if (!string.IsNullOrEmpty(SignalOut) && !string.IsNullOrEmpty(SignalIn))		
				    result += ResultsHeaders[2] + "\n" + ResultsHeaders[6] + "(" +
                        SignalOut + "/" + SignalIn + ")=\n" + SubstitutionResults[2];
                else
                    result += "x(" + SignalOut + ")ч=\n" + SubstitutionResults[2];
			}	

			if (!string.IsNullOrEmpty(SubstitutionResults[5]))
			{
				result += "\n" + ResultsHeaders[7] + "(" + TransRatioOutTxtBox.Text + 
					"/" + TransRatioInTxtBox.Text + ")(" + ResultsHeaders[8] + "=" + 
					ErrorOrder + ")=\n" + SubstitutionResults[5];			
			}
			
			return result;
		}		
		#endregion
		
		#region solve
		private void SolveWithoutErrors()
		{
			int i, j, count;			
			string result = string.Empty, systemDeterminant = string.Empty;

			count = RowArray.GetLength(0);
			string[,] array2Solve = new string[count, count];
            double[,] numberArr = new double[count, count];
			for (j = 0; j < count; j++)
			{
				for (i = 0; i < count; i++)
				{
                    if (RowArray[i].Row[j].Equals("-0"))
                        array2Solve[i, j] = "0";
                    else
    					array2Solve[i, j] = RowArray[i].Row[j];
                    if (!array2Solve[i, j].Equals("0"))
					{
						double.TryParse(array2Solve[i, j], out numberArr[i, j]);
					}				
				}
			}			
			
			if (SystemDeterminant.IsChecked == true)
			{//Визначник системи				
                systemDeterminant = Results[0] = DetCalc.SolvePathApproach(array2Solve, numberArr);
			}		
			if (CompleteTransRatio.IsChecked == true)
			{//Повний коефіцієнт передачі	
				Results[1] = TransRatioCalculation(count - 1, 0, array2Solve, numberArr);					        								
			}
            if (!string.IsNullOrEmpty(SignalOut) &&
                !string.IsNullOrEmpty(SignalIn))
			{//Коефіцієнт передачі
                Results[2] = TransRatioCalculation(int.Parse(SignalOut) - 1, int.Parse(SignalIn) - 1,
				                                         array2Solve, numberArr);			
			}
            else if (!string.IsNullOrEmpty(SignalOut))
            {
                string signal = string.Empty;

                if (string.IsNullOrEmpty(systemDeterminant))
                    systemDeterminant = DetCalc.SolvePathApproach(array2Solve, numberArr);                
                
                signal = TransRatioCalculation(int.Parse(SignalOut) - 1, - 1,
                                                         array2Solve, numberArr);
                Results[2] = "(" + signal + ")/(" + systemDeterminant + ")";
            }
		}        

        private string SolveWithSubstitution()
        {
            int i, j, count;
            bool ok;
            double dbl;
            string cell, tempCell, result, systemDeterminant;
            cell = tempCell = result = systemDeterminant = string.Empty;
            string[,] array2Solve;
            double[,] numberArr;

            Array.Clear(SubstitutionResults, 0, SubstitutionResults.Length);

            if (ModelVars.Count == 0)
                return string.Empty;

            count = ResCount - 3;
            for (i = 0; (i < count) && (string.IsNullOrEmpty(Results[i])); i++) { }
            if (i >= count)
            {
                return "Не розраховано формульний результат";
            }

            j = 0;
            ok = true;
            count = ModelVars.Count;
            for (i = 0; (i < count) && ok; i++) 
            {
                if (ModelVars[i].IsUsed == true)
                {
                    if (!string.IsNullOrEmpty(ModelVars[i].VarValue))
                    {
                        ok = true;
                        j = 1;
                    }
                    else
                        ok = false;
                }
            }
            if (!ok)
                return "Відсутнє значення підстановки";
            else if (j == 0)
                return string.Empty;
            else
            {
                count = RowArray.GetLength(0);
                array2Solve = new string[count, count];
                numberArr = new double[count, count];

                for (j = 0; j < count; j++)
                {
                    for (i = 0; i < count; i++)
                    {
                        cell = RowArray[i].Row[j];
                        if (cell == "-0")
                            cell = "0";
                        if (isSymbolic.IsMatch(cell))
                        {
                            cell = ValueSubstitution(cell);
                        }
                        if (cell != "0")
                        {
                            double.TryParse(cell, out numberArr[i, j]);
                        }
                        array2Solve[i, j] = cell;
                    }
                }

                if (!string.IsNullOrEmpty(Results[0]))
                {//Визначник системи									
                    if (double.TryParse(Results[0], out dbl))
                    {
                        SubstitutionResults[0] = Results[0];
                    }
                    else
                    {
                        systemDeterminant = SubstitutionResults[0] =
                            DetCalc.SolvePathApproach(array2Solve, numberArr);
                    }
                }

                if (!string.IsNullOrEmpty(Results[1]))
                {//Повний коефіцієнт передачі						
                    if (double.TryParse(Results[1], out dbl))
                    {
                        SubstitutionResults[1] = Results[1];
                    }
                    else
                    {
                        SubstitutionResults[1] =
                            TransRatioCalculationSubsit(count - 1, 0, array2Solve, numberArr);
                    }
                }

                if (!string.IsNullOrEmpty(Results[2]))
                {
                    if ((!string.IsNullOrEmpty(SignalOut)) &&
                        (!string.IsNullOrEmpty(SignalIn)))
                    {//Коефіцієнт передачі
                        if (double.TryParse(Results[2], out dbl))
                        {
                            SubstitutionResults[2] = Results[2];
                        }
                        else
                        {
                            SubstitutionResults[2] =
                                TransRatioCalculationSubsit(int.Parse(SignalOut) - 1,
                                int.Parse(SignalIn) - 1, array2Solve, numberArr);
                        }
                    }
                    else
                    {//одиничний сигнал
                        if (double.TryParse(Results[2], out dbl))
                        {
                            SubstitutionResults[2] = Results[2];
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(systemDeterminant))
                                systemDeterminant = DetCalc.SolvePathApproach(array2Solve, numberArr);
                            SubstitutionResults[2] = "(" +
                                TransRatioCalculationSubsit(int.Parse(SignalOut) - 1, -1,
                                array2Solve, numberArr) + ")/(" + systemDeterminant + ")";
                        }
                    }
                }
                return SubstResFormation();
            }
        }

		private string TransRatioCalculation(int outSignal, int inSignal, string[,] array2Solve,
		                                     double[,] numberArr)
		{
			string tempResult1, tempResult2, result;
            tempResult1 = tempResult2 = result = string.Empty;
			int i, count = array2Solve.GetLength(0);
            string[] cells = new string[count];

			for (i = 0; i < count; i++)
			{
                cells[i] = RowArray[i].RowB;
                if (cells[i] == "-0")
                    cells[i] = "0";
                double.TryParse(array2Solve[i, outSignal] = cells[i], out numberArr[i, outSignal]);
			}
            tempResult1 = DetCalc.SolvePathApproach(array2Solve, numberArr);
			for (i = 0; i < count; i++)	
			{
  	           	double.TryParse(array2Solve[i, outSignal] = RowArray[i].Row[outSignal], out numberArr[i, outSignal]);
			}

            if (inSignal != -1)
            {
                for (i = 0; i < count; i++)
                {
                    double.TryParse(array2Solve[i, inSignal] = cells[i], out numberArr[i, inSignal]);
                }
                tempResult2 = DetCalc.SolvePathApproach(array2Solve, numberArr);
                for (i = 0; i < count; i++)
                {
                    double.TryParse(array2Solve[i, inSignal] = RowArray[i].Row[inSignal], out numberArr[i, inSignal]);
                }
                result = "(" + tempResult1 + ")/(" + tempResult2 + ")";
            }
            else
                result = tempResult1;

            return result;
		}

        private string TransRatioCalculationSubsit(int outSignal, int inSignal, string[,] array2Solve,
                                             double[,] numberArr)
        {
            string tempResult1, tempResult2, cell;
            tempResult1 = tempResult2 = cell = string.Empty;
            int i, count, varsCount;
            count = array2Solve.GetLength(0);
            string[] tempArray = new string[count];
            string[] substitArr = new string[count];

            varsCount = ModelVars.Count;
            for (i = 0; i < count; i++)
            {
                tempArray[i] = array2Solve[i, outSignal];
                if (isSymbolic.IsMatch(cell = RowArray[i].RowB))
                {
                    cell = ValueSubstitution(cell);
                }
                if (cell == "-0")
                    cell = "0";
                substitArr[i] = cell;
                double.TryParse(array2Solve[i, outSignal] = cell, out numberArr[i, outSignal]);
            }
            tempResult1 = DetCalc.SolvePathApproach(array2Solve, numberArr);
            for (i = 0; i < count; i++)
            {
                double.TryParse(array2Solve[i, outSignal] = 
                    tempArray[i], out numberArr[i, outSignal]);
            }

            if (inSignal != -1)
            {
                for (i = 0; i < count; i++)
                {
                    tempArray[i] = array2Solve[i, inSignal];
                    double.TryParse(array2Solve[i, inSignal] = 
                        substitArr[i], out numberArr[i, inSignal]);
                }
                tempResult2 = DetCalc.SolvePathApproach(array2Solve, numberArr);
                for (i = 0; i < count; i++)
                {
                    double.TryParse(array2Solve[i, inSignal] = 
                        tempArray[i], out numberArr[i, inSignal]);
                }

                return "(" + tempResult1 + ")/(" + tempResult2 + ")";
            }
            else
                return tempResult1;
        }
		#endregion		
		#endregion
		
		#region event handlers					
		#region toolbar events
		private void NewSession_Click(object sender, RoutedEventArgs e)
		{
			InitModel(true);
		}
		
		private void EditModel_Click(object sender, RoutedEventArgs e)
		{
			InitModel(false);
		}
		
		private void StartBlockScheme_Click(object sender, RoutedEventArgs e)
		{						
			blockSchemeWindow = new BlockSchemeWindow();
			blockSchemeWindow.Owner = this;
			blockSchemeWindow.mainWindow = this;
			blockSchemeWindow.ShowDialog();		
			
        	if (!string.IsNullOrEmpty(BlockSchemePath))
			{
				StateSwitcher(true);				
			}
		}	
		
		private void SetArraySizeBtn_Click(object sender, RoutedEventArgs e)
		{            		
 			if (setArraySizeTextBox.Text == "")
                return;
			try
			{
				SetArrays(Convert.ToInt32(setArraySizeTextBox.Text));
			}
 			catch
 			{
 				MessageBox.Show("Завеликий розмір матриці");
 			}
		}					
		
		private void DetermErrorModel_Checked(object sender, RoutedEventArgs e)
		{
			ErrorModelOrder.IsEnabled = true;
			((TabItem)VarSubst.Items[1]).IsEnabled = true;
		}
		
		private void DetermErrorModel_Unchecked(object sender, RoutedEventArgs e)
		{
			ErrorModelOrder.Text = "";
			ErrorModelOrder.IsEnabled = false;
			((TabItem)VarSubst.Items[1]).IsEnabled = false;
			ErrorOrder = MinusOne;
		}

		private void SolveBtn_Click(object sender, RoutedEventArgs e)
		{
            Array.Clear(Results, 0, Results.Length);
            Array.Clear(SubstitutionResults, 0, SubstitutionResults.Length);
            
            SignalOut = TransRatioOutTxtBox.Text;
            SignalIn = TransRatioInTxtBox.Text;

            if ((SystemDeterminant.IsChecked == false) && (CompleteTransRatio.IsChecked == false) &&
                string.IsNullOrEmpty(TransRatioOutTxtBox.Text))
            {
                determinantTextBox.Text = "Невстановлені кнопки вибору";
                substitutionTextBox.Text = string.Empty;
            }
            else
            {
                determinantTextBox.Text = string.Empty;
                substitutionTextBox.Text = string.Empty;
                SolveWithoutErrors();                
                determinantTextBox.Text = ResultFormation();
                substitutionTextBox.Text = SolveWithSubstitution();
            }
		}	

		private void About_Click(object sender, RoutedEventArgs e)
		{
			Window aboutWindow = new AboutWindow();
			aboutWindow.Owner = this;
			aboutWindow.ShowDialog();				
		}		
		#endregion										
				
		#region expression substitution		
		private void FillBtn_Click(object sender, RoutedEventArgs e)
		{
			if (VarSubst.SelectedIndex == 0)
			{
				int i, j;
				int count = RowArray.GetLength(0);
				double dbl;
				ModelVars.Clear();			
				ModelVarSubst.ItemsSource = null;
				
				for (i = 0; i < count; i++)
				{
					for (j = 0; j < count; j++)
					{
						if (!string.IsNullOrEmpty(RowArray[i].Row[j]))
						{
                            if (!double.TryParse(RowArray[i].Row[j], out dbl))
                            {
                                ModelVarFill(RowArray[i].Row[j]);
                            }
						}
					}			
				}
				
				for (i = 0; i < count; i++)
				{			
					if (!string.IsNullOrEmpty(RowArray[i].RowB))
					{
                        if (!double.TryParse(RowArray[i].RowB, out dbl))
                        {
                            ModelVarFill(RowArray[i].RowB);
                        }
					}	
				}
			
				ModelVarSubst.ItemsSource = ModelVars;
			}
			else if (VarSubst.SelectedIndex == 1)
			{
				ErrorVars.Clear();
				ErrorVarSubst.ItemsSource = null;
				
				if (ModelVars.Count == 0)
				{
					MessageBox.Show("Обновіть спочатку змінні моделі");
					return;
				}
				
				for (int i = 0; i < ModelVars.Count; i++)
				{
					ErrorVars.Add(new ModelVarClass(ErrorVars.Count + 1, 
					                                "d" + ModelVars[i].Variable, SelectAllChechBox.IsChecked));
				}
				
				ErrorVarSubst.ItemsSource = ErrorVars;
			}			
		}		
		
		private void SelectAllChechBox_Checked(object sender, RoutedEventArgs e)
		{
			if (VarSubst.SelectedIndex == 0)
			{			
				for (int i = 0; i < ModelVars.Count; i++)
				{
					ModelVars[i].IsUsed = ((CheckBox)e.OriginalSource).IsChecked;
				}
				ModelVarSubst.Items.Refresh();
				
				IsUsed[0] = ((CheckBox)e.OriginalSource).IsChecked;
			}
			else if (VarSubst.SelectedIndex == 1)
			{	
				for (int i = 0; i < ErrorVars.Count; i++)
				{
					ErrorVars[i].IsUsed = ((CheckBox)e.OriginalSource).IsChecked;
				}
				ErrorVarSubst.Items.Refresh();	

				IsUsed[1] = ((CheckBox)e.OriginalSource).IsChecked;				
			}
		}					
		
		private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			SelectAllChechBox.IsChecked = false;
			if (VarSubst.SelectedIndex == 0)
			{	
				IsUsed[0] = false;
			}
			else if (VarSubst.SelectedIndex == 1)
			{	
				IsUsed[1] = false;
			}
		}

		private void VarSubst_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (((TabControl)sender).SelectedIndex == 0)
			{
				SelectAllChechBox.IsChecked = IsUsed[0];
			}
			else if (((TabControl)sender).SelectedIndex == 1)
			{
				SelectAllChechBox.IsChecked = IsUsed[1];
			}			
		}	
		
		private void ClearBtn_Click(object sender, RoutedEventArgs e)
		{
			if (VarSubst.SelectedIndex == 0)
			{				
				for (int i = 0; i < ModelVars.Count; i++)
				{
					ModelVars[i].VarValue = "";
				}
				ModelVarSubst.Items.Refresh();			
			}
			else if (VarSubst.SelectedIndex == 1)
			{				
				for (int i = 0; i < ErrorVars.Count; i++)
				{
					ErrorVars[i].VarValue = "";
				}
				ErrorVarSubst.Items.Refresh();			
			}	
		}
		
		private void SolvExpressionBtn_Click(object sender, RoutedEventArgs e)
		{
            substitutionTextBox.Text = SolveWithSubstitution();
		}			
		#endregion		
		
        #region open / save events
        private void OpenModelCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;            
        }

        private void SaveModelCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenModelCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        	string exception;
      		ModelItemsToSerialize modelItemsToSerialize = 
      			(ModelItemsToSerialize)OpenSaveClass.OpenDialog(ModelType.Matrix, out exception);
           	if (modelItemsToSerialize == null)
   	    	{
           		if (!string.IsNullOrEmpty(exception))
            	{
   	    			MessageBox.Show("Неможливо відкрити файл моделі внаслідок:\n" + exception);       	    		
           		}
   	    		return;	
   	    	}
	           	
           	this.RowArray = modelItemsToSerialize.RowArray;
	        this.ModelVars = modelItemsToSerialize.ModelVars;
	        this.ErrorVars = modelItemsToSerialize.ErrorVars;
	        this.Results = modelItemsToSerialize.Results;            
            this.SubstitutionResults = modelItemsToSerialize.SubstitutionResults;
            this.BlockSchemePath = modelItemsToSerialize.BlockSchemePath;
            this.SignalOut = modelItemsToSerialize.SignalOut;
            this.SignalIn = modelItemsToSerialize.SignalIn;
            	
			modelItemsToSerialize.Clear();
			modelItemsToSerialize = null;   

			int arraySize = RowArray.Length, count = dataTable.Columns.Count;            				
            
			if (arraySize > count)
	        {
   	            for (int i = count; i < arraySize; i++)
   	            {
   	                column = new DataGridTextColumn();
   	                column.Header = Convert.ToString(i + 1);
   	                column.Binding = new Binding("Row[" + i + "]");
   	                dataTable.Columns.Add(column);                                        
   	            }
   	        }
   	        else if (arraySize < count)
   	        {
   	            for (int i = count - 1; i >= arraySize; i--)
   	            {
   	                dataTable.Columns.RemoveAt(i);
   	            }
   	        }

   	        dataTable.ItemsSource = null;
   	        dataTableXB.ItemsSource = null;
			ModelVarSubst.ItemsSource = null; 
			ErrorVarSubst.ItemsSource = null;			
			dataTable.ItemsSource = RowArray;
   	        dataTableXB.ItemsSource = RowArray;
			ModelVarSubst.ItemsSource = ModelVars;				
			ErrorVarSubst.ItemsSource = ErrorVars;
			
	        if ((!string.IsNullOrEmpty(this.BlockSchemePath)) && (File.Exists(BlockSchemePath)))
	       	{
	        	StateSwitcher(true);
	        }
	        else
	        {
	        	StateSwitcher(false);
	        }
		
			if (blockSchemeWindow != null)
			{
				blockSchemeWindow.WindowDisposal();
				blockSchemeWindow = null;
			}			
		
			determinantTextBox.Clear();
			substitutionTextBox.Clear();				
				
			determinantTextBox.Text = ResultFormation();
			substitutionTextBox.Text = SubstResFormation();
			
			ButtonsReset();
        }
        
        private void SaveModelCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
	    	SaveModelFunction(string.Empty);          
        } 	
        
        private void SaveModelFunction(string blockSchemePath)
        {
        	string filePath = string.Empty;
        	if((filePath = OpenSaveClass.SaveDialog(ModelType.Matrix, blockSchemePath)) != 
        	   string.Empty)
            {         	
	        	ModelItemsToSerialize modelItemsToSerialize = new ModelItemsToSerialize();
	        	modelItemsToSerialize.RowArray = this.RowArray;
		        modelItemsToSerialize.ModelVars = this.ModelVars;
		        modelItemsToSerialize.ErrorVars = this.ErrorVars;
	        	modelItemsToSerialize.Results = this.Results;
   		        modelItemsToSerialize.SubstitutionResults = this.SubstitutionResults;
                modelItemsToSerialize.SignalOut = this.SignalOut;
                modelItemsToSerialize.SignalIn = this.SignalIn;

   		        if (string.IsNullOrEmpty(blockSchemePath))
   		        {
   		        	modelItemsToSerialize.BlockSchemePath = string.Empty;
   		        }
   		        else if (File.Exists(blockSchemePath))
   		        {
   		        	modelItemsToSerialize.BlockSchemePath = blockSchemePath;
   		        }
   		        else
   		        {
   		        	modelItemsToSerialize.BlockSchemePath = string.Empty;
   		        	StateSwitcher(false);
   		        }
   		        string exception;
   		        if (!OpenSaveClass.SaveCommand(filePath, modelItemsToSerialize, out exception))
		        {
   		       		MessageBox.Show("Неможливо зберегти модель в файл внаслідок:\n" + exception);
  		       	}
    	   		modelItemsToSerialize = null;        	   		        
        	}
        }
        
		private void SaveAllCmdExecuted(object sender, ExecutedRoutedEventArgs e)
		{				
			string exception, filePath;
			exception = filePath = string.Empty;
			if (!string.IsNullOrEmpty(BlockSchemePath))
			{
				if (File.Exists(BlockSchemePath))
				{
					if ((filePath = OpenSaveClass.SaveDialog(ModelType.Scheme, string.Empty)) != 
					    string.Empty)
					{
		 				if (OpenSaveClass.SaveCommand(BlockSchemePath, filePath, out exception))
						{
		 					SaveModelFunction(filePath);
		 				}
		 				else
		 				{		 					
							MessageBox.Show("Неможливо зберегти схему в файл внаслідок:\n" + 
		 					                exception);
		 					SaveModelFunction(string.Empty);
						}	 						
					}
				}
				else
				{
					StateSwitcher(false);
				}
			}		
		}     
		#endregion
		
		#region text restrictions		
		private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
		}
		
		private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space) e.Handled = true;
		}						

		private void TransRatioTxtBox_LostFocus(object sender, RoutedEventArgs e)
		{
			string str = ((TextBox)sender).Text;
			if (string.IsNullOrEmpty(str) || str.Equals("0"))
			{
				((TextBox)sender).Text = string.Empty;	
				e.Handled = true;
				return;				
			}			
			
			int val;
			if ((int.TryParse(str, out val)) && (val > dataTable.Columns.Count))
			{
				MessageBox.Show("Число перевищує розмір матриці");
				((TextBox)sender).Text = string.Empty;					
			}
			e.Handled = true;
		}				

		private void ErrorModelOrder_MouseLeave(object sender, MouseEventArgs e)
		{
			if (string.IsNullOrEmpty(((TextBox)sender).Text) || ((TextBox)sender).Text.Equals("0"))
			{
				((TextBox)sender).Text = string.Empty;
				DetermErrorModel.IsChecked = false;
				ErrorOrder = MinusOne;		
				e.Handled = true;
				return;				
			}
			
			int val;
			if ((int.TryParse(((TextBox)sender).Text, out val)) && (val <= dataTable.Columns.Count))
			{				
				ErrorOrder = val;
			}
			else
			{
				MessageBox.Show("Число перевищує розмір матриці");
				((TextBox)sender).Text = string.Empty;						
				DetermErrorModel.IsChecked = false;
				ErrorOrder = MinusOne;
			}
			e.Handled = true;
		}
		
		private void DataTable_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
            string cellVal = ((TextBox)e.EditingElement).Text;
			if (string.IsNullOrEmpty(cellVal))
			{
				((TextBox)e.EditingElement).Text = "0";
			}
            else if (cellVal.Contains("\b"))
            {
                ((TextBox)e.EditingElement).Text = cellVal.Replace("\b", "");
            }
		}																	
		#endregion				
        #endregion
    }
}