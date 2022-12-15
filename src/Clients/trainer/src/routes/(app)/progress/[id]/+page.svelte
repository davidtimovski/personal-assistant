<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';
	import { page } from '$app/stores';

	import { DateHelper } from '../../../../../../shared2/utils/dateHelper';
	import { ValidationResult, ValidationUtil } from '../../../../../../shared2/utils/validationUtils';
	import AlertBlock from '../../../../../../shared2/components/AlertBlock.svelte';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { alertState } from '$lib/stores';

	import EditProgressAmountForm from '$lib/components/EditProgressAmountForm.svelte';

	export let data: PageData;

	const isNew = data.id === 0;

	const exercise = {
		id: 0,
		name: 'Running',
		sets: 2,
		type: 'amount',
		amountUnit: 'meters'
	};

	onMount(() => {
		exercise.id = parseInt(<string>$page.url.searchParams.get('exerciseId'), 10);
	});

	onDestroy(() => {});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-pencil-alt" />
		</div>
		<div class="page-title">
			<!-- <span>{$t(isNew ? 'editUpcomingExpense.newUpcomingExpense' : 'editUpcomingExpense.editUpcomingExpense')}</span> -->
		</div>
		<a href="/" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		{#if exercise.type === 'amount'}
			<EditProgressAmountForm
				id={data.id}
				exerciseId={exercise.id}
				sets={exercise.sets}
				amountUnit={exercise.amountUnit}
			/>
		{/if}
	</div>
</section>
