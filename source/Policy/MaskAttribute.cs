using System;
using System.Collections.Generic;

namespace WM.SmarterFoodSelection
{
	// Wasn't sure if I should use a generic type of not.
	public class MaskAttribute /* where T : IEquatable<T> */
	{
		protected string name; // should be null for bool
		protected bool unspecified = true;

		object value_int;

		public MaskAttribute(object defaultValue)
		{
			//this.name = "unnamedattribute_" + defaultValue.GetType().Name;
			value_int = defaultValue;
		}
		public MaskAttribute(string name, object defaultValue)
		{
			this.name = name;
			value_int = defaultValue;
		}
		//public MaskAttribute()
		//{
		//	this.name = "unnamedattribute";
		//}

		public bool Unspecified
		{
			get
			{
				return unspecified;
			}
		}
		public string Name
		{
			get
			{
				if (name == "")
					return value_int.GetType().Name;
				return name;
			}
		}

		public string GetValueName()
		{
			if (name != null)
				return name;

			return value_int.ToString();
		}

		public object Value
		{
			get
			{
				if (unspecified)
					return null;
				return value_int;
			}
			set
			{
				unspecified = false;
				this.value_int = value;
			}
		}

		public bool Matches(object value)
		{
			if (unspecified)
				return true;

			if (this.Value.Equals(value))
				return true;

			return false;
		}

		//public override bool Equals(object obj)
		//{
		//	var value = obj as MaskAttribute;

		//	if (value == null)
		//		return false;

		//	if (value.unspecified != this.unspecified)
		//	{
		//		return false;
		//	}
		//	if (value.unspecified)
		//		return true;

		//	if (!value.Value.Equals(this.Value))
		//		return false;

		//	return true;
		//}
		//public override int GetHashCode()
		//{
		//	return base.GetHashCode();
		//}

		public void SetValueNamed(string valueName)
		{
			Value = GetValueNamed(valueName);
		}
		public object GetValueNamed(string valueName)
#if DEBUG
		{
			var value = _GetValueNamed(valueName);

			//Log.Message("Possible values of " + name + " : " + string.Join(";", value.ToArray()));

			return value;
		}
		public object _GetValueNamed(string valueName)
#endif
		{
			if (value_int is bool)
			{
				if (valueName.CaseUnsensitiveCompare(name))
					return true;
				return null;
			}

			if (value_int.GetType().IsEnum)
			{
				var values = new List<string>();

				// can't use linq...
				foreach (var entry in (Enum.GetValues(value_int.GetType())))
				{
					if (entry.ToString().CaseUnsensitiveCompare(valueName))
						return entry;
				}
				return null;
			}

			throw new InvalidOperationException("Can not determine the list of the possible texted values. type : " + value_int.GetType().Name);
		}
		public override string ToString()
		{
			return string.Format("[MaskAttribute: Type={0}, Name={1}, Value={2}]", value_int.GetType().Name, Name, Value);
		}

		//public static implicit operator MaskAttribute(object obj)
		//{
		//	return new MaskAttribute(obj);
		//}
	}

}
