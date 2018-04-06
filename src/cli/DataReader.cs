using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OPESchemes;
using Simulation;

namespace CLI
{
	/// <summary>
	/// I - index (plaintext) type
	/// D - data type
	/// </summary>
	public class DataReader<I, D>
	{
		public Inputs<I, D> Inputs = new Inputs<I, D>();

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

					Inputs.Dataset.Add(new Record<I, D>(ConvertToType<I>(record[0]), ConvertToType<D>(record[1])));
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
							Inputs.ExactQueries.Add(new ExactQuery<I>(ConvertToType<I>(line)));
							break;
						case QueriesType.Range:
							Inputs.RangeQueries.Add(new RangeQuery<I>(ConvertToType<I>(line.Split(',')[0]), ConvertToType<I>(line.Split(',')[1])));
							break;
						case QueriesType.Update:
							Inputs.UpdateQueries.Add(new UpdateQuery<I, D>(ConvertToType<I>(line.Split(',')[0]), ConvertToType<D>(line.Split(',')[1])));
							break;
						case QueriesType.Delete:
							Inputs.DeleteQueries.Add(new DeleteQuery<I>(ConvertToType<I>(line)));
							break;
						default:
							throw new InvalidOperationException($"Type {type} is not supported");
					}
				}
			}
		}

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
