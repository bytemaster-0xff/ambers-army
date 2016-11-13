using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ATTM2X
{
	public static class Extensions
	{
		/// <summary>
		/// Returns all fields declared in type and its base
		/// </summary>
		public static IEnumerable<FieldInfo> GetFields(this Type type)
		{
			Type t = type;
			do
			{
				TypeInfo ti = t.GetTypeInfo();
				foreach (var p in ti.DeclaredFields)
					yield return p;
				t = ti.BaseType;
			} while (t != null);
		}

		/// <summary>
		/// Returns all properties declared in type and its base
		/// </summary>
		public static IEnumerable<PropertyInfo> GetProperties(this Type type)
		{
			Type t = type;
			do
			{
				TypeInfo ti = t.GetTypeInfo();
				foreach (var p in ti.DeclaredProperties)
					yield return p;
				t = ti.BaseType;
			} while (t != null);
		}

		public static string[] GetResourceIdentifiersWithIdIncluding(this M2XDevice basis, bool includeId = true, params string[] parms)
		{
			var resourceParts = new List<string> { M2XDevice.UrlPath };
			if (includeId)
			{
				resourceParts.Add(basis.DeviceId);
			}
			if (parms != null && parms.Any()) { resourceParts.AddRange(parms); }
			return resourceParts.ToArray();
		}

		public static string[] GetResourceIdentifiersWithIdIncluding(this M2XDistribution basis, bool includeId = true, params string[] parms)
		{
			var resourceParts = new List<string> { M2XDistribution.UrlPath };
			if (includeId) { resourceParts.Add(basis.DistributionId); }
			if (parms != null && parms.Any()) { resourceParts.AddRange(parms); }
			return resourceParts.ToArray();
		}

		public static string[] GetResourceIdentifiersWithIdIncluding(this M2XStream basis, bool startFromParent = true, bool includeId = true, params string[] parms)
		{
			var resourceParts = new List<string>();
			if (startFromParent && basis.Device != null)
			{
				resourceParts.Add(M2XDevice.UrlPath);
				resourceParts.Add(basis.Device.DeviceId);
			}

			resourceParts.Add(M2XStream.UrlPath);
			if (includeId) { resourceParts.Add(basis.StreamName); }

			if (parms != null && parms.Any()) { resourceParts.AddRange(parms); }
			return resourceParts.ToArray();
		}
	}
}