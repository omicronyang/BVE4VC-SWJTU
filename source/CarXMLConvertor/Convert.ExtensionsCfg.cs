﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Path = OpenBveApi.Path;

namespace CarXmlConvertor
{
	class ConvertExtensionsCfg
	{
		internal static string FileName;
		internal static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

		internal static void Process()
		{
			if (!System.IO.File.Exists(FileName))
			{
				//No extensions.cfg file exists, so just spin up a default XML file
				GenerateDefaultXML();
			}
			else
			{
				CarInfos = new Car[ConvertTrainDat.NumberOfCars];
				ReadExtensionsCfg();
				GenerateExtensionsCfgXML();
			}
		}

		private struct Car
		{
			internal double Length;
			internal double FrontAxle;
			internal double RearAxle;
			internal bool AxlesDefined;
			internal bool Reversed;
			internal string Object;
			internal Bogie FrontBogie;
			internal Bogie RearBogie;
		}

		private struct Bogie
		{
			//Need a second struct to avoid a circular reference
			internal double Length;
			internal double FrontAxle;
			internal double RearAxle;
			internal bool AxlesDefined;
			internal bool Reversed;
			internal string Object;
		}


		private static Car[] CarInfos;
	

		internal static void ReadExtensionsCfg()
		{
			string[] Lines = System.IO.File.ReadAllLines(FileName, System.Text.Encoding.Default);
			for (int i = 0; i < Lines.Length; i++)
			{
				int j = Lines[i].IndexOf(';');
				if (j >= 0)
				{
					Lines[i] = Lines[i].Substring(0, j).Trim();
				}
				else
				{
					Lines[i] = Lines[i].Trim();
				}
			}
			for (int i = 0; i < Lines.Length; i++)
			{
				if (Lines[i].Length != 0)
				{
					switch (Lines[i].ToLowerInvariant())
					{
						case "[exterior]":
							// exterior
							i++;
							while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								if (Lines[i].Length != 0)
								{
									int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
									if (j >= 0)
									{
										string a = Lines[i].Substring(0, j).TrimEnd();
										string b = Lines[i].Substring(j + 1).TrimStart();
										int n;
										if (int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out n))
										{
											if (n >= 0 & n < ConvertTrainDat.NumberOfCars)
											{
												if (!String.IsNullOrEmpty(b) && !Path.ContainsInvalidChars(b))
												{
													string File = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), b);
													if (System.IO.File.Exists(File))
													{
														CarInfos[n].Object = b;
													}
												}
											}
										}
									}
								}
								i++;
							}
							i--;
							break;
						default:
							if (Lines[i].StartsWith("[car", StringComparison.OrdinalIgnoreCase) & Lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								// car
								string t = Lines[i].Substring(4, Lines[i].Length - 5);
								int n; if (int.TryParse(t, System.Globalization.NumberStyles.Integer, Culture, out n))
								{
									if (n >= 0 & n < ConvertTrainDat.NumberOfCars)
									{
										i++;
										while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal))
										{
											if (Lines[i].Length != 0)
											{
												int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
												if (j >= 0)
												{
													string a = Lines[i].Substring(0, j).TrimEnd();
													string b = Lines[i].Substring(j + 1).TrimStart();
													switch (a.ToLowerInvariant())
													{
														case "object":
															if (!String.IsNullOrEmpty(b) && !Path.ContainsInvalidChars(b))
															{
																string File = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), b);
																if (System.IO.File.Exists(File))
																{
																	CarInfos[n].Object = b;
																}
															}
															break;
														case "length":
														{
															double m;
															if (double.TryParse(b, System.Globalization.NumberStyles.Float, Culture, out m))
															{
																if (m > 0.0)
																{
																	CarInfos[n].Length = m;
																}
															}
														}
															break;
														case "axles":
														int k = b.IndexOf(',');
															if (k >= 0)
															{
																string c = b.Substring(0, k).TrimEnd();
																string d = b.Substring(k + 1).TrimStart();
																double rear, front;
																if (double.TryParse(c, System.Globalization.NumberStyles.Float, Culture, out rear) && double.TryParse(d, System.Globalization.NumberStyles.Float, Culture, out front))
																{
																	CarInfos[n].RearAxle = rear;
																	CarInfos[n].FrontAxle = front;
																	CarInfos[n].AxlesDefined = true;
																}
															}
															break;
														case "reversed":
															CarInfos[n].Reversed = b.Equals("true", StringComparison.OrdinalIgnoreCase);
															break;
													}
												}
											}
											i++;
										}
										i--;
									}
								}
							}
							/*
							else if (Lines[i].StartsWith("[coupler", StringComparison.OrdinalIgnoreCase) & Lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								// coupler
								string t = Lines[i].Substring(8, Lines[i].Length - 9);
								int n; if (int.TryParse(t, System.Globalization.NumberStyles.Integer, Culture, out n))
								{
									if (n >= 0 & n < Train.Couplers.Length)
									{
										i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal))
										{
											if (Lines[i].Length != 0)
											{
												int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
												if (j >= 0)
												{
													string a = Lines[i].Substring(0, j).TrimEnd();
													string b = Lines[i].Substring(j + 1).TrimStart();
													switch (a.ToLowerInvariant())
													{
														case "distances":
														{
															int k = b.IndexOf(',');
															if (k >= 0)
															{
																string c = b.Substring(0, k).TrimEnd();
																string d = b.Substring(k + 1).TrimStart();
																double min, max;
																if (!double.TryParse(c, System.Globalization.NumberStyles.Float, Culture, out min))
																{
																	Interface.AddMessage(Interface.MessageType.Error, false, "Minimum is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																}
																else if (!double.TryParse(d, System.Globalization.NumberStyles.Float, Culture, out max))
																{
																	Interface.AddMessage(Interface.MessageType.Error, false, "Maximum is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																}
																else if (min > max)
																{
																	Interface.AddMessage(Interface.MessageType.Error, false, "Minimum is expected to be less than Maximum in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																}
																else
																{
																	Train.Couplers[n].MinimumDistanceBetweenCars = min;
																	Train.Couplers[n].MaximumDistanceBetweenCars = max;
																}
															}
															else
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "An argument-separating comma is expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
														}
															break;
														default:
															Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key-value pair " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															break;
													}
												}
												else
												{
													Interface.AddMessage(Interface.MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												}
											}
											i++;
										}
										i--;
									}
									else
									{
										Interface.AddMessage(Interface.MessageType.Error, false, "The coupler index " + t + " does not reference an existing coupler at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								}
								else
								{
									Interface.AddMessage(Interface.MessageType.Error, false, "The coupler index is expected to be an integer at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
							}
							*/
							else if (Lines[i].StartsWith("[bogie", StringComparison.OrdinalIgnoreCase) & Lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								// car
								string t = Lines[i].Substring(6, Lines[i].Length - 7);
								int n; if (int.TryParse(t, System.Globalization.NumberStyles.Integer, Culture, out n))
								{
									//Assuming that there are two bogies per car
									bool IsOdd = (n % 2 != 0);
									int CarIndex = n / 2;
									if (n >= 0 & n < ConvertTrainDat.NumberOfCars * 2)
									{
										bool DefinedAxles = false;
										i++;
										while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal))
										{
											if (Lines[i].Length != 0)
											{
												int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
												if (j >= 0)
												{
													string a = Lines[i].Substring(0, j).TrimEnd();
													string b = Lines[i].Substring(j + 1).TrimStart();
													switch (a.ToLowerInvariant())
													{
														case "object":
															if (!String.IsNullOrEmpty(b) && !Path.ContainsInvalidChars(b))
															{
																string File = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), b);
																if (System.IO.File.Exists(File))
																{
																	if (IsOdd)
																	{
																		CarInfos[CarIndex].FrontBogie.Object = b;
																	}
																	else
																	{
																		CarInfos[CarIndex].RearBogie.Object = b;
																	}
																}
															}
															break;
														case "axles":
															int k = b.IndexOf(',');
															if (k >= 0)
															{
																string c = b.Substring(0, k).TrimEnd();
																string d = b.Substring(k + 1).TrimStart();
																double rear, front;
																if (double.TryParse(c, System.Globalization.NumberStyles.Float, Culture, out rear) && double.TryParse(d, System.Globalization.NumberStyles.Float, Culture, out front))
																{
																	if (IsOdd)
																	{
																		CarInfos[CarIndex].FrontBogie.RearAxle = rear;
																		CarInfos[CarIndex].FrontBogie.FrontAxle = front;
																		CarInfos[CarIndex].FrontBogie.AxlesDefined = true;
																	}
																	else
																	{
																		CarInfos[CarIndex].RearBogie.RearAxle = rear;
																		CarInfos[CarIndex].RearBogie.FrontAxle = front;
																		CarInfos[CarIndex].RearBogie.AxlesDefined = true;
																	}
																}
															}
															break;
														case "reversed":
															if (IsOdd)
															{
																CarInfos[n].FrontBogie.Reversed = b.Equals("true", StringComparison.OrdinalIgnoreCase);
															}
															else
															{
																CarInfos[n].RearBogie.Reversed = b.Equals("true", StringComparison.OrdinalIgnoreCase);
															}
															break;
													}
												}
											}
											i++;
										}
										i--;
										
									}
								}
							}
							break;
					}
				}
			}
		}

		internal static void GenerateExtensionsCfgXML()
		{
			List<string> newLines = new List<string>();
			newLines.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			newLines.Add(
				"<openBVE xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
			newLines.Add("<Train>");
			for (int i = 0; i < ConvertTrainDat.NumberOfCars; i++)
			{
				newLines.Add("<Car>");
				if (CarInfos[i].Length != 0.0)
				{
					newLines.Add("<Length>" + CarInfos[i].Length + "</Length>");
				}
				else
				{
					newLines.Add("<Length>" + ConvertTrainDat.CarLength + "</Length>");
				}
				newLines.Add("<Width>" + ConvertTrainDat.CarWidth + "</Width>");
				newLines.Add("<Height>" + ConvertTrainDat.CarHeight + "</Height>");
				if (ConvertTrainDat.MotorCars[i] == true)
				{
					newLines.Add("<MotorCar>True</MotorCar>");
					newLines.Add("<Mass>" + ConvertTrainDat.MotorCarMass + "</Mass>");
				}
				else
				{
					newLines.Add("<MotorCar>False</MotorCar>");
					newLines.Add("<Mass>" + ConvertTrainDat.TrailerCarMass + "</Mass>");
				}
				if (CarInfos[i].AxlesDefined == true)
				{
					newLines.Add("<FrontAxle>" + CarInfos[i].FrontAxle + "</FrontAxle>");
					newLines.Add("<RearAxle>" + CarInfos[i].RearAxle + "</RearAxle>");
				}
				else
				{
					newLines.Add("<FrontAxle>" + 0.4 * ConvertTrainDat.CarLength + "</FrontAxle>");
					newLines.Add("<RearAxle>" + -(0.4 * ConvertTrainDat.CarLength) + "</RearAxle>");
				}
				if (!String.IsNullOrEmpty(CarInfos[i].Object))
				{
					newLines.Add("<Object>" + CarInfos[i].Object + "</Object>");
				}
				newLines.Add("<Reversed>" + CarInfos[i].Reversed + "</Reversed>");
				if (CarInfos[i].FrontBogie.AxlesDefined == true || !string.IsNullOrEmpty(CarInfos[i].FrontBogie.Object))
				{
					newLines.Add("<FrontBogie>");
					newLines.Add("<FrontAxle>" + CarInfos[i].FrontBogie.FrontAxle + "</FrontAxle>");
					newLines.Add("<RearAxle>" + CarInfos[i].FrontBogie.RearAxle + "</RearAxle>");
					newLines.Add("<Object>" + CarInfos[i].FrontBogie.Object + "</Object>");
					newLines.Add("<Reversed>" + CarInfos[i].FrontBogie.Reversed + "</Reversed>");
					newLines.Add("</FrontBogie>");
				}

				if (CarInfos[i].RearBogie.AxlesDefined == true || !string.IsNullOrEmpty(CarInfos[i].RearBogie.Object))
				{
					newLines.Add("<RearBogie>");
					newLines.Add("<FrontAxle>" + CarInfos[i].RearBogie.FrontAxle + "</FrontAxle>");
					newLines.Add("<RearAxle>" + CarInfos[i].RearBogie.RearAxle + "</RearAxle>");
					newLines.Add("<Object>" + CarInfos[i].RearBogie.Object + "</Object>");
					newLines.Add("<Reversed>" + CarInfos[i].RearBogie.Reversed + "</Reversed>");
					newLines.Add("</RearBogie>");
				}
				newLines.Add("</Car>");
			}
			newLines.Add("</Train>");
			newLines.Add("</openBVE>");
			try
			{
				string fileOut = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FileName), "Train.xml");
				using (StreamWriter sw = new StreamWriter(fileOut))
				{
					foreach (String s in newLines)
						sw.WriteLine(s);
				}
			}
			catch
			{
				MessageBox.Show("An error occured whilst writing the new XML file. \r\n Please check for write permissions.", "CarXML Convertor", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			MessageBox.Show("Conversion succeeded.", "CarXML Convertor", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		/// <summary>Generates a train.xml file using the values / assumptions contained within the train.dat file</summary>
		internal static void GenerateDefaultXML()
		{
			List<string> newLines = new List<string>();
			newLines.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			newLines.Add("<openBVE xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
			newLines.Add("<Train>");
			for (int i = 0; i < ConvertTrainDat.NumberOfCars; i++)
			{
				newLines.Add("<Car>");
				newLines.Add("<Length>" + ConvertTrainDat.CarLength + "</Length>");
				newLines.Add("<Width>" + ConvertTrainDat.CarWidth + "</Width>");
				newLines.Add("<Height>" + ConvertTrainDat.CarHeight + "</Height>");
				if (ConvertTrainDat.MotorCars[i] == true)
				{
					newLines.Add("<MotorCar>True</MotorCar>");
					newLines.Add("<Mass>" + ConvertTrainDat.MotorCarMass + "</Mass>");
				}
				else
				{
					newLines.Add("<MotorCar>False</MotorCar>");
					newLines.Add("<Mass>" + ConvertTrainDat.TrailerCarMass + "</Mass>");
				}
				newLines.Add("<FrontAxle>" + 0.4 * ConvertTrainDat.CarLength + "</FrontAxle>");
				newLines.Add("<RearAxle>" + -(0.4 * ConvertTrainDat.CarLength) + "</RearAxle>");
				newLines.Add("</Car>");
			}
			newLines.Add("</Train>");
			newLines.Add("</openBVE>");
			try
			{
				string fileOut = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FileName), "Train.xml");
				using (StreamWriter sw = new StreamWriter(fileOut))
				{
					foreach (String s in newLines)
						sw.WriteLine(s);
				}
			}
			catch
			{
				MessageBox.Show("An error occured whilst writing the new XML file. \r\n Please check for write permissions.", "CarXML Convertor", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			MessageBox.Show("Conversion succeeded.", "CarXML Convertor", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}
}
