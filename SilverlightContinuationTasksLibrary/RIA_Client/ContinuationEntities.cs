using System;
using System.Collections.Generic;
using System.ServiceModel.DomainServices.Client;

namespace ContinuationTasks.RIA_Client
{
	public class ContinuationEntities<T> where T : DomainContext
	{
		private T _DomainContext;
		private TaskQueue _DelayTaskQueue;
		private int _SubmitInterval;

		public ContinuationEntities(T domainContext, int submitInterval = 100 /* milliseconds */)
		{
			_DomainContext = domainContext;
			_DelayTaskQueue = new TaskQueue();
			_SubmitInterval = submitInterval;
		}

		public void SubmitEntities<T1>(IEnumerable<T1> entities,
			Action<SubmitOperation> onSubmittedEachChange = null, Action onSubmittedAllChanges = null,
			bool clearEntityContainer = true) where T1 : Entity
		{
			EntitySet<T1> entitySet = _DomainContext.EntityContainer.GetEntitySet<T1>();
			IEnumerator<T1> toAddEntities = entities.GetEnumerator();

			if (clearEntityContainer)
				entitySet.Clear();

			AddEntity(entitySet, toAddEntities, onSubmittedEachChange, onSubmittedAllChanges);
		}

		private void AddEntity<T1>(EntitySet<T1> entitySet, IEnumerator<T1> toAddEntities,
			Action<SubmitOperation> onSubmittedEachChange, Action onSubmittedAllChanges)
			where T1 : Entity
		{
			if (toAddEntities.MoveNext())
			{
				T1 addingEntity = toAddEntities.Current;

				_DelayTaskQueue.RunSingleTask(() =>
				{
					entitySet.Add(addingEntity);

					_DomainContext.SubmitChanges(op =>
					{
						if (onSubmittedEachChange != null)
							onSubmittedEachChange(op);

						AddEntity(entitySet, toAddEntities, onSubmittedEachChange, onSubmittedAllChanges);

					}, addingEntity);
				}, _SubmitInterval, () => entitySet.HasChanges, 1);
			}
			else
				if (onSubmittedAllChanges != null)
					onSubmittedAllChanges();
		}

		public void ClearEntitySet<T1>() where T1 : Entity
		{
			_DomainContext.EntityContainer.GetEntitySet<T1>().Clear();
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Copyright 2012 Abel Cheng
//	This source code is subject to terms and conditions of the MIT License.
//	You must not remove this notice, or any other, from this software.
//
//	Original Author:	Abel Cheng <abelcys@gmail.com>
//	Created Date:		2012-12-19
//	Source Host:		http://sctl.codeplex.com
//	Change Log:
//	Author				Date			Comment
//
//
//
//
//	(Keep Code Clean)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
