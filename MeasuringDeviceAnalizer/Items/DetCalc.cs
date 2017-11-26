using System;
using System.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MeasuringDeviceAnalizer
{
	/// <summary>
	/// Решение определителей в символьном виде
    /// Нахождение определителей матриц
    /// с помощью алгебры Грассмана
	/// </summary>

	public static class DetCalc
	{
        public static string SolvePathApproach(string[,] inDataArray, double[,] numberArr)
        {
            #region variables
            int curCol, curRow, prevRow, m, k, i, startColNum;
			int zeroCount = 0;
            double dblNum;
            bool isContains;	

			//длина пути
			int N = inDataArray.GetLength(0);

			//массив номеров столбцов ненулевых ячеек inDataArray
            List<int>[] columnNumArr = new List<int>[N];

            //numberArr - массив подтверждения наличия ненулевых чисел в inDataArray

			//массив номеров столбцов текущего пути в nonzeroArray
			int[] curPath = new int[N];	
			int[] curRealPath = new int[N];
            int pathVarsCount = 0;
            string[] curRealPathVars = new string[N];
            int[] curRealPathVarsDup = new int[N];

            //список слагаемых
            List<List<string>> determinant = new List<List<string>>();
            List<double> numDeterminant = new List<double>();
            int determinantRowCount;

            //parsers
            Regex isMaltiVar = new Regex(@"[-+*/()]{1,}?");

			//временные ответы
			string tempResult = string.Empty;
			double tempNumResult = 0;
			
			//ответ
			string result = string.Empty;
            #endregion

            #region preparing
            //определение номеров столбцов ненулевых ячеек inDataArray
            //и поиск нулевых рядков
			for (curRow = 0; curRow < N; curRow++)
			{
				zeroCount = 0;
                columnNumArr[curRow] = new List<int>();
				for (curCol = 0; curCol < N; curCol++)
				{
					if (!string.Equals(inDataArray[curRow, curCol], "0") &&
                        !string.Equals(inDataArray[curRow, curCol], "-0"))
					{
						columnNumArr[curRow].Add(curCol);						                       
					}
					else
						zeroCount++;
				}
				if (zeroCount == N)
				{
					return "0";
				}								
			}
			
			for (curCol = 0; curCol < N; curCol++)
			{//поиск нулевых столбцов
				zeroCount = 0;
				
				for (curRow = 0; curRow < N; curRow++)
				{
					if (string.Equals( inDataArray[curRow, curCol], "0"))
						zeroCount++;
				}
				if (zeroCount == N)
				{
					return "0";
				}
            }
            #endregion

            #region path formation
            //Формирование траекторий									
            for (startColNum = 0; (startColNum < columnNumArr[0].Count); startColNum++)
			{				
				curPath[0] = startColNum;
				curRealPath[0] = columnNumArr[0][startColNum];
				curRow = 1; curCol = 0;
				while ((curRow < N)&&(curRow > 0))
				{
                    while ((curRow < N) && (curCol < columnNumArr[curRow].Count))
					{
						prevRow = curRow - 1;
						while((prevRow > -1) && (curRealPath[prevRow] != columnNumArr[curRow][curCol]))
            			{
                			prevRow -= 1;
            			}     
            			if(prevRow < 0)
            			{
            				curPath[curRow] = curCol;
            				curRealPath[curRow] = columnNumArr[curRow][curCol];
                			curRow++;
                			curCol = 0;
            			}
            			else
            			{
                			curCol++;
            			}
					}
					if (curRow >= N)
					{//путь построен
						#region Вывод в результат						
                        //очистка временных данных
						tempNumResult = 1d;						
						tempResult = string.Empty;
                        Array.Clear(curRealPathVars, 0, N);
                        Array.Clear(curRealPathVarsDup, 0, N);
                        pathVarsCount = 0;

                        //получение множителей	
                        for (i = 0; i < N; i++)
						{
							if (numberArr[i, curRealPath[i]] != 0)
								tempNumResult *= numberArr[i, curRealPath[i]];
							else
							{
                                tempResult = inDataArray[i, curRealPath[i]];
                                if (tempResult.StartsWith("-"))
                                {
                                    tempNumResult *= -1d;
                                    tempResult = tempResult.Substring(1);
                                }                               

                                if ((k = Array.IndexOf(curRealPathVars, tempResult)) == -1)
                                {                                    
                                    curRealPathVars[pathVarsCount] = tempResult;
                                    pathVarsCount++;
                                }
                                else
                                    curRealPathVarsDup[k]++;
							}
						}
                        
                        if (pathVarsCount > 0)
                        {
                            for (i = 0; i < pathVarsCount; i++)
                            {
                                if (curRealPathVarsDup[i] > 0)
                                    if (isMaltiVar.IsMatch(curRealPathVars[i]))
                                        curRealPathVars[i] = "(" + curRealPathVars[i] + ")" +
                                            "^" + Convert.ToString(curRealPathVarsDup[i] + 1);
                                    else
                                        curRealPathVars[i] = curRealPathVars[i] +
                                            "^" + Convert.ToString(curRealPathVarsDup[i] + 1);
                            }
                        }
                        else
                            determinant.Add(new List<string>(1) { string.Empty } );

						//Поиск количества беспорядков в пути
						//для определения знака 
						i = 0;
						for (k = 0; k < (N - 1); k++)
        				{
							for (m = (k + 1); m < N; m++)
            				{
								if(curRealPath[k] > curRealPath[m])
                    				i++;
            				}
        				}						
						if ((i % 2) != 0)
							if (tempNumResult == 0d)
								tempNumResult = -1d;
							else
								tempNumResult *= -1d;                        

                        isContains = false;
                        determinantRowCount = determinant.Count;
                        if ((pathVarsCount > 0) && (determinantRowCount > 0))
                        {                            
                            for (k = 0; (k < determinantRowCount) && (!isContains); k++)
                            {
                                if ((determinant[k].Count - 1) == pathVarsCount)
                                    for (i = 0; (i < pathVarsCount) && (isContains =
                                        determinant[k].Contains(curRealPathVars[i])); i++) { }
                            }
                        }
                        if (isContains)
                        {
                            k--;
                            if (string.IsNullOrEmpty(determinant[k][0]))
                                determinant[k][0] = tempNumResult.ToString();
                            else if ((dblNum = double.Parse(determinant[k][0]) + tempNumResult) != 0d)
                                determinant[k][0] = dblNum.ToString();
                            else
                                determinant.RemoveAt(k);
                        }
                        else
                        {
                            determinant.Add(new List<string>(pathVarsCount + 1) 
                                { tempNumResult.ToString() });
                            k = determinant.Count - 1;
                            for (i = 0; i < pathVarsCount; i++)
                            {
                                determinant[k].Add(curRealPathVars[i]);
                            }
                        }
                        #endregion
                    }
						
					curRow--;
					curCol = curPath[curRow] + 1;
				}
            }
            #endregion

            #region results formation
            determinantRowCount = determinant.Count;
            result = tempResult = string.Empty;
            for (k = 0; k < determinantRowCount; k++)
            {
                tempResult = string.Join("*", determinant[k]);
                if (Regex.IsMatch(tempResult, @"^-1\*"))
                    result += Regex.Replace(tempResult, @"^-1\*", "-");
                else if (Regex.IsMatch(tempResult, @"^1\*"))
                    result += Regex.Replace(tempResult, @"^1\*", "+");
                else if (Regex.IsMatch(tempResult, @"^[^-]"))
                    result += "+" + tempResult;
                else
                    result += tempResult;
            }

            if (string.IsNullOrEmpty(result))
                return "0";
            if (result.StartsWith("+"))
                return result.Substring(1);
            else
                return result;
            #endregion            
        }
	}
}