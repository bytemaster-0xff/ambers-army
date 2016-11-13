using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbersArmy.Core.Interfaces
{
	public interface IM2XMqttService
	{
		Task PostStreamValues(string deviceId, string streamName, string values);
		Task PostStreamValues<T>(string deviceId, string streamName, List<T> values);
	}
}