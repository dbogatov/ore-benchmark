using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Simulation;

namespace CLI
{
	/// <typeparam name="D">Data type</typeparam>
	public class DataReader<D>
	{
		public Inputs<D> Inputs = new Inputs<D>();

		/// <summary>
		/// Immediately populates its local lists with the data read from files
		/// </summary>
		public DataReader(string dataset, string queries, QueriesType type)
		{
			Inputs.Type = type;

			using (StreamReader sr = File.OpenText(dataset))
			{
				string line;

				while ((line = sr.ReadLine()) != null)
				{
					var record = line.Split(',');

					Inputs.Dataset.Add(new Record<D>(ConvertToType<int>(record[0]), ConvertToType<D>(record[1])));
				}
			}

			using (StreamReader sr = File.OpenText(queries))
			{
				string line;

				while ((line = sr.ReadLine()) != null)
				{
					switch (type)
					{
						case QueriesType.Exact:
							Inputs.ExactQueries.Add(new ExactQuery(ConvertToType<int>(line)));
							break;
						case QueriesType.Range:
							Inputs.RangeQueries.Add(new RangeQuery(ConvertToType<int>(line.Split(',')[0]), ConvertToType<int>(line.Split(',')[1])));
							break;
						case QueriesType.Update:
							Inputs.UpdateQueries.Add(new UpdateQuery<D>(ConvertToType<int>(line.Split(',')[0]), ConvertToType<D>(line.Split(',')[1])));
							break;
						case QueriesType.Delete:
							Inputs.DeleteQueries.Add(new DeleteQuery(ConvertToType<int>(line)));
							break;
						default:
							throw new InvalidOperationException($"Type {type} is not supported");
					}
				}
			}
		}

		/// <summary>
		/// Converts string value to the specified type
		/// </summary>
		/// <param name="value">Value to convert</param>
		/// <returns>The input value but of a proper type</returns>
		private T ConvertToType<T>(string value)
		{
			switch (Type.GetTypeCode(typeof(T)))
			{
				case TypeCode.String:
					return (T)(object)value;
				case TypeCode.Int32:
					return (T)(object)int.Parse(value);
				default:
					throw new NotImplementedException($"Type {value.GetType()} is not implemented");
			}
		}
	}
}
