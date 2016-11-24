﻿using System.Threading.Tasks;
using RawRabbit.Operations.Saga.Repository;
using Stateless;

namespace RawRabbit.Operations.Saga.Model
{
	public abstract class Saga
	{
		public abstract Task TriggerAsync(object trigger);
		public abstract Task TriggerAsync<TPayload>(object trigger, TPayload payload);
		public abstract SagaDto GetDto();
	}

	public abstract class  Saga<TState, TTrigger> : Saga<TState, TTrigger, SagaDto<TState>> { }

	public abstract class Saga<TState, TTrigger, TSagaDto> : Saga where TSagaDto : SagaDto<TState>
	{
		protected readonly TSagaDto SagaDto;
		protected StateMachine<TState, TTrigger> StateMachine;
		protected TriggerParameterRepository<TState, TTrigger> TriggerParameters;

		protected Saga(TSagaDto sagaDto = null)
		{
			SagaDto = sagaDto ?? Initialize();
			StateMachine = new StateMachine<TState, TTrigger>(() => SagaDto.State, s => SagaDto.State = s);
			TriggerParameters = new TriggerParameterRepository<TState, TTrigger>(StateMachine);
			ConfigureState(StateMachine);
		}

		protected abstract void ConfigureState(StateMachine<TState, TTrigger> machine);

		public abstract TSagaDto Initialize();

		public override Task TriggerAsync(object trigger)
		{
			return StateMachine.FireAsync((TTrigger) trigger);
		}

		public override Task TriggerAsync<TPayload>(object trigger, TPayload payload)
		{
			var paramTrigger = TriggerParameters.Get<TPayload>((TTrigger)trigger);
			return StateMachine.FireAsync(paramTrigger, payload);
		}
		
		public override SagaDto GetDto()
		{
			return SagaDto;
		}
	}
}