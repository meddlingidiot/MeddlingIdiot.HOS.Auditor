using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;

namespace MeddlingIdiot.HOS.TimelineNavigator.Extensions
{

	public static class GenericExtensions
	{
		public static T CanSerialize<T>(this T value)
		{
			var DCS = new DataContractSerializer(typeof(T));
			byte[] bytes;
			using (var ms = new MemoryStream())
			{
				DCS.WriteObject(ms, value);
				bytes = ms.ToArray();
			}

			T? result;
			using (var ms = new MemoryStream(bytes))
			{
				result = (T?)DCS.ReadObject(ms);
			}

			return result!;
		}

		public static bool DeepEquals(this object? objectA, object? objectB, params string[] ignoreList)
		{
			bool result;

			if (objectA != null && objectB != null)
			{
				Type objectType;

				objectType = objectA.GetType();

				result = true; // assume by default they are equal

				foreach (PropertyInfo propertyInfo in objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && !ignoreList.Contains(p.Name)))
				{
					object? valueA;
					object? valueB;

					valueA = propertyInfo.GetValue(objectA, null);
					valueB = propertyInfo.GetValue(objectB, null);

					// if it is a primative type, value type or implements IComparable, just directly try and compare the value
					if (CanDirectlyCompare(propertyInfo.PropertyType))
					{
						if (!AreValuesEqual(valueA, valueB))
						{
							Console.WriteLine("Mismatch with property '{0}.{1}' found.", objectType.FullName, propertyInfo.Name);
							result = false;
						}
					}
					// if it implements IEnumerable, then scan any items
					else if (typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
					{
						IEnumerable<object> collectionItems1;
						IEnumerable<object> collectionItems2;
						int collectionItemsCount1;
						int collectionItemsCount2;

						// null check
						if (valueA == null && valueB != null || valueA != null && valueB == null)
						{
							Console.WriteLine("Mismatch with property '{0}.{1}' found.", objectType.FullName, propertyInfo.Name);
							result = false;
						}
						else if (valueA != null && valueB != null)
						{
							collectionItems1 = ((IEnumerable)valueA).Cast<object>().ToList();
							collectionItems2 = ((IEnumerable)valueB).Cast<object>().ToList();
							collectionItemsCount1 = collectionItems1.Count();
							collectionItemsCount2 = collectionItems2.Count();

							// check the counts to ensure they match
							if (collectionItemsCount1 != collectionItemsCount2)
							{
								Console.WriteLine("Collection counts for property '{0}.{1}' do not match.", objectType.FullName, propertyInfo.Name);
								result = false;
							}
							// and if they do, compare each item... this assumes both collections have the same order
							else
							{
								for (int i = 0; i < collectionItemsCount1; i++)
								{
									object collectionItem1;
									object collectionItem2;
									Type collectionItemType;

									collectionItem1 = collectionItems1.ElementAt(i);
									collectionItem2 = collectionItems2.ElementAt(i);
									collectionItemType = collectionItem1.GetType();

									if (CanDirectlyCompare(collectionItemType))
									{
										if (!AreValuesEqual(collectionItem1, collectionItem2))
										{
											Console.WriteLine("Item {0} in property collection '{1}.{2}' does not match.", i, objectType.FullName, propertyInfo.Name);
											result = false;
										}
									}
									else if (!DeepEquals(collectionItem1, collectionItem2, ignoreList))
									{
										Console.WriteLine("Item {0} in property collection '{1}.{2}' does not match.", i, objectType.FullName, propertyInfo.Name);
										result = false;
									}
								}
							}
						}
					}
					else if (propertyInfo.PropertyType.IsClass)
					{
						if (!DeepEquals(propertyInfo.GetValue(objectA, null), propertyInfo.GetValue(objectB, null), ignoreList))
						{
							Console.WriteLine("Mismatch with property '{0}.{1}' found.", objectType.FullName, propertyInfo.Name);
							result = false;
						}
					}
					else
					{
						Console.WriteLine("Cannot compare property '{0}.{1}'.", objectType.FullName, propertyInfo.Name);
						result = false;
					}
				}
			}
			else
				result = object.Equals(objectA, objectB);

			return result;
		}

		private static bool AreValuesEqual(object? objValue, object? otherValue)
		{
			bool result;
			IComparable? selfValueComparer;

			selfValueComparer = objValue as IComparable;

			if (objValue == null && otherValue != null || objValue != null && otherValue == null)
				result = false;
			else if (selfValueComparer != null && selfValueComparer.CompareTo(otherValue) != 0)
				result = false;
			else if (!object.Equals(objValue, otherValue))
				result = false;
			else
				result = true;

			return result;
		}

		private static bool CanDirectlyCompare(Type propertyType) => typeof(IComparable).IsAssignableFrom(propertyType) || propertyType.IsPrimitive || propertyType.IsValueType;
		

		public static T? DeepClone<T>(this T obj)
		{
			if (obj == null)
				throw new ArgumentNullException("Object is null");
			return (T?)CloneProcedure(obj);
		}

		private static object? CloneProcedure(object? obj)
		{
			if (obj == null)
				return null;

			Type type = obj.GetType();

			if (type.IsPrimitive || type.IsEnum || type == typeof(string))
			{
				return obj;
			}
			else if (type.IsArray)
			{
				Type? typeElement = Type.GetType(type.FullName?.Replace("[]", string.Empty) ?? string.Empty);
				var array = obj as Array;
				Array copiedArray = Array.CreateInstance(typeElement!, array!.Length);
				for (int i = 0; i < array.Length; i++)
				{
					copiedArray.SetValue(CloneProcedure(array.GetValue(i)), i);
				}

				return copiedArray;
			}
			else if (type.IsClass || type.IsValueType)
			{
				if (type.Name == "clsStateClass") return obj;
				object? copiedObject = Activator.CreateInstance(obj.GetType());
				FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				foreach (FieldInfo field in fields)
				{
					object? fieldValue = field.GetValue(obj);
					if (fieldValue != null)
					{
						field.SetValue(copiedObject, CloneProcedure(fieldValue));
					}
				}
				return copiedObject;
			}
			else
			{
				throw new ArgumentException("The object is unknown type.");
			}
		}
	}
}
