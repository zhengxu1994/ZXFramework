using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bepop.Core
{
    public static class AnimatorExtension
    {
		public static void AddAnimatorParameterIfExists(this Animator animator, string parameterName, AnimatorControllerParameterType type, List<string> parameterList)
		{
			if (animator.HasParameterOfType(parameterName, type))
			{
				parameterList.Add(parameterName);
			}
		}

		// <summary>
		/// Updates the animator bool.
		/// </summary>
		/// <param name="self">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		public static void UpdateAnimatorBool(this Animator self, string parameterName,bool value, List<string> parameterList)
		{
			if (parameterList.Contains(parameterName))
			{
				self.SetBool(parameterName,value);
			}
		}

		public static void UpdateAnimatorTrigger(this Animator self, string parameterName, List<string> parameterList)
		{
			if (parameterList.Contains(parameterName))
			{
				self.SetTrigger(parameterName);
			}
		}

		/// <summary>
		/// Triggers an animator trigger.
		/// </summary>
		/// <param name="self">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		public static void SetAnimatorTrigger(this Animator self, string parameterName, List<string> parameterList)
		{
			if (parameterList.Contains(parameterName))
			{
				self.SetTrigger(parameterName);
			}
		}
		
		/// <summary>
		/// Updates the animator float.
		/// </summary>
		/// <param name="self">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">Value.</param>
		public static void UpdateAnimatorFloat(this Animator self, string parameterName,float value, List<string> parameterList)
		{
			if (parameterList.Contains(parameterName))
			{
				self.SetFloat(parameterName,value);
			}
		}
		
		/// <summary>
		/// Updates the animator integer.
		/// </summary>
		/// <param name="self">self.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">Value.</param>
		public static void UpdateAnimatorInteger(this Animator self, string parameterName,int value, List<string> parameterList)
		{
			if (parameterList.Contains(parameterName))
			{
				self.SetInteger(parameterName,value);
			}
		}	 




		// <summary>
		/// Updates the animator bool without checking the parameter's existence.
		/// </summary>
		/// <param name="self">self.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		public static void UpdateAnimatorBool(this Animator self, string parameterName,bool value)
		{
			self.SetBool(parameterName,value);
		}

		/// <summary>
		/// Updates the animator trigger without checking the parameter's existence
		/// </summary>
		/// <param name="self">self.</param>
		/// <param name="parameterName">Parameter name.</param>
		public static void UpdateAnimatorTrigger(this Animator self, string parameterName)
		{
			self.SetTrigger(parameterName);
		}

		/// <summary>
		/// Triggers an animator trigger without checking for the parameter's existence.
		/// </summary>
		/// <param name="self">self.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		public static void SetAnimatorTrigger(this Animator self, string parameterName)
		{
			self.SetTrigger(parameterName);
		}
		
		/// <summary>
		/// Updates the animator float without checking for the parameter's existence.
		/// </summary>
		/// <param name="self">self.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">Value.</param>
		public static void UpdateAnimatorFloat(this Animator self, string parameterName,float value)
		{
			self.SetFloat(parameterName,value);
		}
		
		/// <summary>
		/// Updates the animator integer without checking for the parameter's existence.
		/// </summary>
		/// <param name="self">self.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">Value.</param>
		public static void UpdateAnimatorInteger(this Animator self, string parameterName,int value)
		{
			self.SetInteger(parameterName,value);
		}  




		// <summary>
		/// Updates the animator bool after checking the parameter's existence.
		/// </summary>
		/// <param name="self">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		public static void UpdateAnimatorBoolIfExists(this Animator self, string parameterName,bool value)
		{
			if (self.HasParameterOfType(parameterName, AnimatorControllerParameterType.Bool))
			{
				self.SetBool(parameterName,value);	
			}
		}

		public static void UpdateAnimatorTriggerIfExists(this Animator self, string parameterName)
		{
			if (self.HasParameterOfType(parameterName, AnimatorControllerParameterType.Trigger))
			{
				self.SetTrigger(parameterName);
			}
		}

		/// <summary>
		/// Triggers an animator trigger after checking for the parameter's existence.
		/// </summary>
		/// <param name="self">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		public static void SetAnimatorTriggerIfExists(this Animator self, string parameterName)
		{
			if (self.HasParameterOfType(parameterName, AnimatorControllerParameterType.Trigger))
			{
				self.SetTrigger(parameterName);
			}
		}

		/// <summary>
		/// Updates the animator float after checking for the parameter's existence.
		/// </summary>
		/// <param name="self">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">Value.</param>
		public static void UpdateAnimatorFloatIfExists(this Animator self, string parameterName,float value)
		{
			if (self.HasParameterOfType(parameterName, AnimatorControllerParameterType.Float))
			{
				self.SetFloat(parameterName,value);
			}
		}

		/// <summary>
		/// Updates the animator integer after checking for the parameter's existence.
		/// </summary>
		/// <param name="self">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">Value.</param>
		public static void UpdateAnimatorIntegerIfExists(this Animator self, string parameterName,int value)
		{
			if (self.HasParameterOfType(parameterName, AnimatorControllerParameterType.Int))
			{
				self.SetInteger(parameterName,value);
			}
		}    
		
		/// <summary>
		/// Determines if an animator contains a certain parameter, based on a type and a name
		/// </summary>
		/// <returns><c>true</c> if has parameter of type the specified self name type; otherwise, <c>false</c>.</returns>
		/// <param name="self">Self.</param>
		/// <param name="name">Name.</param>
		/// <param name="type">Type.</param>
		public static bool HasParameterOfType (this Animator self, string name, AnimatorControllerParameterType type) 
		{
			if (string.IsNullOrEmpty(name)) { return false; }
			var parameters = self.parameters;
			return parameters.Any(currParam => currParam.type == type && currParam.name == name);
		}
    }
}