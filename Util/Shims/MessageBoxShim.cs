using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.VisualBasic;

namespace Util.Shims;
public record MessageBoxAttempt(string Message, string? Title, MsgBoxStyle Style);
public class MessageBoxShim
{
	private MessageBoxShim()
	{
		
	}

	public static MessageBoxShim GetSparkShim(DrMessageBoxListener underlying)
	{
		return new MessageBoxShim()
		{
			SparkUnderlying = underlying,
		};
	}
	public static MessageBoxShim GetDrShim()
	{
		if (Util.ContextInformation.IsWeb)
		{
			// You are seeing this because this shim is not set up to forward messages to Spark
			// This type of shim is to be used, where DR will call, but never Spark.
			// To fix this, follow the callstack up, and replace the GetDrShim, with GetDeadMessageBoxShim
			Debugger.Break();
			throw new InvalidOperationException("Cant use DR Shim, in Web!");
		}
		return new MessageBoxShim();
	}

	public static MessageBoxShim GetDeadMessageBoxShim()
	{
		return new MessageBoxShim()
		{
			isDeadShim = true
		};
	}

	private bool isDeadShim;
	private DrMessageBoxListener? SparkUnderlying;
	
	public MsgBoxResult MsgBox(
		object Prompt,
		MsgBoxStyle Buttons = MsgBoxStyle.ApplicationModal,
		object? Title = null)
	{
		if (ContextInformation.IsWeb)
		{
			if (isDeadShim)
			{
				// We are missing this message :(
				
				// You are seeing this because this is a 'fake' shim to forward messages to Spark
				// These shims can't actually forward the message, because the plumbing was never set up.
				// Your program will run fine, but you will not be able to observe this message in Spark.
				//
				// You can try and modify the DR code to prevent the MsgBox from firing, and returning the message
				// in some other means to Spark. Or to change the MessageBoxShim to the real kind. Probably ask Tyler,
				// You can comment this out, but try not to commit it.
				Debugger.Break();
			}

			if (SparkUnderlying is not null)
			{
				// Spark is going to try and also read these messages
				if (Prompt is string stringPrompt)
				{
					SparkUnderlying.Enque(new MessageBoxAttempt(
						stringPrompt,
						Title as string,
						Buttons));
				}
				else
				{
					// Prompt wasn't a string?
					Debugger.Break();
				}
			}
			// The web will default err on the side of least resistance, I.E. what is likely to keep the program going
			// and not stuck in a "do thing again" loop.

#pragma warning disable RCS1258 // Unnecessary enum flag - Verbose for clarity.
			const MsgBoxStyle AllInputTypes =
				MsgBoxStyle.OkOnly
				| MsgBoxStyle.OkCancel
				| MsgBoxStyle.AbortRetryIgnore
				| MsgBoxStyle.YesNoCancel
				| MsgBoxStyle.YesNo
				| MsgBoxStyle.RetryCancel;
#pragma warning restore RCS1258 // Unnecessary enum flag - Verbose for clarity.

			if ((Buttons & AllInputTypes) == 0)
			{
				// No input set, t.f. either OkOnly or a default - User is always "ok".
				return MsgBoxResult.Ok;
			}
			else if (Buttons.HasFlag(MsgBoxStyle.OkCancel))
			{
				return MsgBoxResult.Ok;
			}
			else if (Buttons.HasFlag(MsgBoxStyle.AbortRetryIgnore))
			{
				return MsgBoxResult.Abort;
			}
			else if (Buttons.HasFlag(MsgBoxStyle.YesNoCancel) || Buttons.HasFlag(MsgBoxStyle.YesNo))
			{
				return MsgBoxResult.Yes;
			}
			else if (Buttons.HasFlag(MsgBoxStyle.RetryCancel))
			{
				return MsgBoxResult.Cancel;
			}

			// The above branch stack is exhaustive, but the compiler can't tell :(
			return MsgBoxResult.Ok;
		}
		else
		{
			return Interaction.MsgBox(Prompt, Buttons, Title);
		}
	}
}
