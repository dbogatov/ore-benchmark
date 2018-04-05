using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Simulation;

namespace CLI
{
	public class DataReader<T, C>
	{
		public Inputs<T, C> Inputs = new Inputs<T, C>();

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

					Inputs.Dataset.Add(new Record<T, C>(ConvertToType<T>(record[0]), ConvertToType<C>(record[1])));
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
							Inputs.ExactQueries.Add(new ExactQuery<T>(ConvertToType<T>(line)));
							break;
						case QueriesType.Range:
							Inputs.RangeQueries.Add(new RangeQuery<T>(ConvertToType<T>(line.Split(',')[0]), ConvertToType<T>(line.Split(',')[1])));
							break;
						case QueriesType.Update:
							Inputs.UpdateQueries.Add(new UpdateQuery<T, C>(ConvertToType<T>(line.Split(',')[0]), ConvertToType<C>(line.Split(',')[1])));
							break;
						case QueriesType.Delete:
							Inputs.DeleteQueries.Add(new DeleteQuery<T>(ConvertToType<T>(line)));
							break;
						default:
							throw new InvalidOperationException($"Type {type} is not supported");
					}
				}
			}
		}

		private D ConvertToType<D>(string value)
		{
			switch (Type.GetTypeCode(typeof(D)))
			{
				case TypeCode.String:
					return (D)(object)value;
				case TypeCode.Int32:
					return (D)(object)int.Parse(value);
				default:
					throw new NotImplementedException($"Type {value.GetType()} is not implemented");
			}
		}
	}
}
