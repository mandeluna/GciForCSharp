using Util.Shims;

namespace System
{
	public interface IForwarder
	{
		void Forward(MessageBoxAttempt messageBoxAttempt);
	}

	public class DoNothingFowarder : IForwarder
	{
		public void Forward(MessageBoxAttempt messageBoxAttempt)
		{
			// Do nothing
		}
	}
	
}
