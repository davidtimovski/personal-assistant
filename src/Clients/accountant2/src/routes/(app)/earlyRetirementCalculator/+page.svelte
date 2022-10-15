<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';

	import { CurrenciesService } from '../../../../../shared2/services/currenciesService';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { Formatter } from '$lib/utils/formatter';
	import { locale } from '$lib/stores';
	import { AccountsService } from '$lib/services/accountsService';
	import { LargeUpcomingExpense, SummaryItem } from '$lib/models/viewmodels/earlyRetirementCalculator';

	import AmountInput from '$lib/components/AmountInput.svelte';
	import Checkbox from '$lib/components/Checkbox.svelte';

	// Answers
	let age: number;
	let capital: number | null = null;
	let capitalCurrency: string;
	let savedPerMonth: number | null = null;
	let savedPerMonthCurrency: string;
	let savingInterestRate: number;
	let eligibleForPension = false;
	let pensionAge: number;
	let pensionPerMonth: number | null = null;
	let pensionPerMonthCurrency: string;
	let hasLifeInsurance = false;
	let lifeInsuranceAge: number;
	let lifeInsuranceReturn: number | null = null;
	let lifeInsuranceReturnCurrency: string;
	let upcomingExpenses: LargeUpcomingExpense[] = [];
	let retirementIncome: number | null = null;
	let retirementIncomeCurrency: string;
	let consideringTheAnswersMessage: string;

	let currentSection = 'start';
	const sections = [
		'start',
		'age',
		'capital',
		'saving',
		'pension-eligibility',
		'pension',
		'have-life-insurance',
		'life-insurance',
		'upcoming-expenses',
		'retirement-income',
		'summary',
		'result'
	];
	let currency: string;
	let ageIsInvalid: boolean;
	let pensionAgeIsInvalid: boolean;
	let pensionPerMonthIsInvalid: boolean;
	let lifeInsuranceAgeIsInvalid: boolean;
	let lifeInsuranceReturnIsInvalid: boolean;
	let preferredRetirementIncomeIsInvalid: boolean;
	let earlyRetirementAge: number;
	let summaryItems: SummaryItem[];
	const ageOfDeath = 85;
	const inflation = 0.02;

	let localStorage: LocalStorageUtil;
	let accountsService: AccountsService;
	let currenciesService: CurrenciesService;

	function addUpcomingExpense() {
		upcomingExpenses = upcomingExpenses.concat(new LargeUpcomingExpense('', 'fas fa-wallet', currency));
	}

	function removeUpcomingExpense(upcomingExpense: LargeUpcomingExpense) {
		upcomingExpenses = upcomingExpenses.filter((x) => x !== upcomingExpense);
	}

	function start() {
		currentSection = 'age';
	}

	function goToCapital() {
		ageIsInvalid = false;

		if (!age || age.toString().trim() === '') {
			ageIsInvalid = true;
			return;
		}

		currentSection = 'capital';
	}

	function goToSaving() {
		currentSection = 'saving';
	}

	function goToPensionEligibility() {
		currentSection = 'pension-eligibility';
	}

	function goToPension() {
		if (eligibleForPension) {
			currentSection = 'pension';
		} else {
			currentSection = 'have-life-insurance';
		}
	}

	function goToHaveLifeInsurance() {
		pensionAgeIsInvalid = false;
		pensionPerMonthIsInvalid = false;

		if (!pensionAge || pensionAge.toString().trim() === '') {
			pensionAgeIsInvalid = true;
			return;
		}
		if (!pensionPerMonth || pensionPerMonth.toString().trim() === '') {
			pensionPerMonthIsInvalid = true;
			return;
		}

		currentSection = 'have-life-insurance';
	}

	function goToLifeInsurance() {
		if (hasLifeInsurance) {
			currentSection = 'life-insurance';
		} else {
			currentSection = 'upcoming-expenses';
		}
	}

	function goToUpcomingExpenses() {
		lifeInsuranceAgeIsInvalid = false;
		lifeInsuranceReturnIsInvalid = false;

		if (!lifeInsuranceAge || lifeInsuranceAge.toString().trim() === '') {
			lifeInsuranceAgeIsInvalid = true;
			return;
		}
		if (!lifeInsuranceReturn || lifeInsuranceReturn.toString().trim() === '') {
			lifeInsuranceReturnIsInvalid = true;
			return;
		}

		currentSection = 'upcoming-expenses';
	}

	function goToRetirementIncome() {
		upcomingExpenses = upcomingExpenses.filter((x) => !!x.amount);

		currentSection = 'retirement-income';
	}

	function goToSummary() {
		preferredRetirementIncomeIsInvalid = false;

		if (!retirementIncome || retirementIncome.toString().trim() === '') {
			preferredRetirementIncomeIsInvalid = true;
			return;
		}

		summaryItems = [];

		summaryItems.push(new SummaryItem($t('earlyRetirementCalculator.summaryItem1', { age: age })));

		if (capital && parseFloat(capital.toString()) > 0) {
			summaryItems.push(
				new SummaryItem(
					$t('earlyRetirementCalculator.summaryItem2a', {
						capital: Formatter.money(capital, capitalCurrency, $locale)
					})
				)
			);
		} else {
			summaryItems.push(new SummaryItem($t('earlyRetirementCalculator.summaryItem2b')));
		}

		if (savedPerMonth && parseFloat(savedPerMonth.toString()) > 0) {
			summaryItems.push(
				new SummaryItem(
					$t('earlyRetirementCalculator.summaryItem3a', {
						savedPerMonth: Formatter.money(savedPerMonth, savedPerMonthCurrency, $locale),
						savingInterestRate: savingInterestRate
					})
				)
			);
		} else {
			summaryItems.push(new SummaryItem($t('earlyRetirementCalculator.summaryItem3b')));
		}

		if (eligibleForPension) {
			summaryItems.push(
				new SummaryItem(
					$t('earlyRetirementCalculator.summaryItem4a', {
						pensionAge: pensionAge,
						pensionPerMonth: Formatter.money(pensionPerMonth, pensionPerMonthCurrency, $locale)
					})
				)
			);
		} else {
			summaryItems.push(new SummaryItem($t('earlyRetirementCalculator.summaryItem4b')));
		}

		if (hasLifeInsurance) {
			summaryItems.push(
				new SummaryItem(
					$t('earlyRetirementCalculator.summaryItem5a', {
						lifeInsuranceReturn: Formatter.money(lifeInsuranceReturn, lifeInsuranceReturnCurrency, $locale),
						lifeInsuranceAge: lifeInsuranceAge
					})
				)
			);
		} else {
			summaryItems.push(new SummaryItem($t('earlyRetirementCalculator.summaryItem5b')));
		}

		if (upcomingExpenses.length > 0) {
			const expensesSummaryItem = new SummaryItem($t('earlyRetirementCalculator.summaryItem6'));

			for (const expense of upcomingExpenses) {
				expensesSummaryItem.children.push(
					new SummaryItem(
						$t('earlyRetirementCalculator.summaryItem6a', {
							amount: Formatter.money(expense.amount, expense.currency, $locale),
							expense: expense.name
						})
					)
				);
			}

			summaryItems.push(expensesSummaryItem);
		}

		summaryItems.push(
			new SummaryItem(
				$t('earlyRetirementCalculator.summaryItem7', {
					retirementIncome: Formatter.money(retirementIncome, retirementIncomeCurrency, $locale)
				})
			)
		);

		summaryItems.push(new SummaryItem($t('earlyRetirementCalculator.summaryItem8', { inflation: inflation * 100 })));

		currentSection = 'summary';
	}

	function back() {
		const currentIndex = sections.indexOf(currentSection);

		switch (currentSection) {
			case 'have-life-insurance':
				{
					if (eligibleForPension) {
						currentSection = sections[currentIndex - 1];
					} else {
						currentSection = sections[currentIndex - 2];
					}
				}
				break;
			case 'upcoming-expenses':
				{
					if (hasLifeInsurance) {
						currentSection = sections[currentIndex - 1];
					} else {
						currentSection = sections[currentIndex - 2];
					}
				}
				break;
			default:
				currentSection = sections[currentIndex - 1];
		}
	}

	function calculate() {
		const largeUpcomingExpensesSum = upcomingExpenses
			.map((x) => getConvertedValueOrZero(<number>x.amount, x.currency))
			.reduce((a: number, b: number) => a + b, 0);

		let pensionSum = 0;
		if (eligibleForPension) {
			const yearsTakingPension = ageOfDeath - getIntValueOrZero(pensionAge);
			const pension = getConvertedValueOrZero(<number>pensionPerMonth, pensionPerMonthCurrency);
			pensionSum = yearsTakingPension * 12 * pension;
		}

		lifeInsuranceAge = getIntValueOrZero(lifeInsuranceAge);
		lifeInsuranceReturn = hasLifeInsurance
			? getConvertedValueOrZero(<number>lifeInsuranceReturn, lifeInsuranceReturnCurrency)
			: 0;

		const monthlyInflationFactor = 1 - inflation / 12;
		const savingInterestMonthlyFactor = 1 + getFloatValueOrZero(savingInterestRate) / 100 / 12;
		savedPerMonth = getConvertedValueOrZero(<number>savedPerMonth, savedPerMonthCurrency);

		let currentAge = parseInt(age.toString(), 10);
		let saved = getConvertedValueOrZero(<number>capital, capitalCurrency);

		let requiredCapital = sumRequiredCapital(currentAge, largeUpcomingExpensesSum, pensionSum);

		while (true) {
			if (saved >= requiredCapital) {
				break;
			}

			for (let i = 0; i < 12; i++) {
				saved *= monthlyInflationFactor;
				saved *= savingInterestMonthlyFactor;
				saved += savedPerMonth;
			}

			if (hasLifeInsurance && currentAge === lifeInsuranceAge) {
				saved += lifeInsuranceReturn;
			}

			currentAge++;
			if (currentAge === ageOfDeath) {
				break;
			}

			requiredCapital = sumRequiredCapital(currentAge, largeUpcomingExpensesSum, pensionSum);
		}

		earlyRetirementAge = currentAge;

		if (earlyRetirementAge < 85) {
			consideringTheAnswersMessage = $t('earlyRetirementCalculator.consideringTheAnswers', {
				earlyRetirementAge: earlyRetirementAge
			});
		}

		const currentIndex = sections.indexOf(currentSection);
		currentSection = sections[currentIndex + 1];
	}

	function sumRequiredCapital(currentAge: number, largeUpcomingExpensesSum: number, pensionSum: number) {
		const monthsInRetirement = (ageOfDeath - currentAge) * 12;
		retirementIncome = getConvertedValueOrZero(<number>retirementIncome, retirementIncomeCurrency);
		const monthlyInflation = inflation / 12;

		let retirementAmountNeeded = 0;
		for (let i = 0; i < monthsInRetirement; i++) {
			retirementAmountNeeded += retirementIncome;
			retirementAmountNeeded *= 1 + monthlyInflation;
		}

		return retirementAmountNeeded + largeUpcomingExpensesSum - pensionSum;
	}

	function getIntValueOrZero(value: number): number {
		if (!value || value.toString().trim() === '') {
			return 0;
		}

		return parseInt(value.toString(), 10);
	}

	function getFloatValueOrZero(value: number): number {
		if (!value || value.toString().trim() === '') {
			return 0;
		}

		return parseFloat(value.toString());
	}

	function getConvertedValueOrZero(amount: number, amountCurrency: string): number {
		if (!amount || amount.toString().trim() === '') {
			return 0;
		}

		const parsed = parseFloat(amount.toString());

		return currenciesService.convert(parsed, amountCurrency, currency);
	}

	onMount(async () => {
		localStorage = new LocalStorageUtil();
		accountsService = new AccountsService();
		currenciesService = new CurrenciesService('Accountant');

		currency = localStorage.get(LocalStorageKeys.Currency);

		capitalCurrency =
			savedPerMonthCurrency =
			pensionPerMonthCurrency =
			lifeInsuranceReturnCurrency =
			retirementIncomeCurrency =
				currency;

		upcomingExpenses = [
			new LargeUpcomingExpense($t('earlyRetirementCalculator.home'), 'fas fa-home', currency),
			new LargeUpcomingExpense($t('earlyRetirementCalculator.car'), 'fas fa-car', currency),
			new LargeUpcomingExpense($t('earlyRetirementCalculator.kids'), 'fas fa-baby', currency)
		];

		accountsService.getAllWithBalance(currency).then((accounts) => {
			const balance = accounts.map((x) => <number>x.balance).reduce((a: number, b: number) => a + b, 0);
			capital = Math.floor(balance);
		});

		accountsService.getAverageMonthlySavingsFromThePastYear(currency).then((savingsPerMonth) => {
			savedPerMonth = Math.floor(savingsPerMonth);
		});
	});

	onDestroy(() => {
		accountsService?.release();
		currenciesService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive medium">
			<i class="fas fa-piggy-bank" />
		</div>
		<div class="page-title">{$t('earlyRetirementCalculator.earlyRetirementCalculator')}</div>
		<a href="/" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<section class="step" class:shown={currentSection === 'start'}>
			<p class="er-calc-explanation">{$t('earlyRetirementCalculator.explanation')}</p>

			<div class="centering-wrap">
				<button type="button" on:click={start} class="er-calc-button big start"
					>{$t('earlyRetirementCalculator.start')}</button
				>
			</div>
		</section>

		<section class="step" class:shown={currentSection === 'age'}>
			<div class="section-title">{$t('earlyRetirementCalculator.age')}</div>

			<div class="question">{$t('earlyRetirementCalculator.whatIsYourAge')}</div>
			<div class="er-calc-input-wrap">
				<div class="interest-rate-input-wrap">
					<input type="number" bind:value={age} class:invalid={ageIsInvalid} min="10" max="99" required />
					<span class="add-on">{$t('earlyRetirementCalculator.years')}</span>
				</div>
			</div>

			<div class="buttons-wrap">
				<button type="button" on:click={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" on:click={goToCapital} class="er-calc-button">
					{$t('earlyRetirementCalculator.next')}
				</button>
			</div>
		</section>

		<section class="step" class:shown={currentSection === 'capital'}>
			<div class="section-title">{$t('earlyRetirementCalculator.capital')}</div>

			<div class="question">
				{$t('earlyRetirementCalculator.howMuchMoneyCapital')}
			</div>
			<div class="question-detail">
				{$t('earlyRetirementCalculator.weHavePrepopulatedCapital')}
			</div>
			<div class="er-calc-input-wrap">
				<AmountInput bind:amount={capital} bind:currency={capitalCurrency} />
			</div>

			<div class="buttons-wrap">
				<button type="button" on:click={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" on:click={goToSaving} class="er-calc-button">
					{$t('earlyRetirementCalculator.next')}
				</button>
			</div>
		</section>

		<section class="step" class:shown={currentSection === 'saving'}>
			<div class="section-title">{$t('earlyRetirementCalculator.saving')}</div>

			<div class="question">
				{$t('earlyRetirementCalculator.howMuchMoneySaved')}
			</div>
			<div class="question-detail">
				{$t('earlyRetirementCalculator.weHavePrepopulatedSaving')}
			</div>
			<div class="er-calc-input-wrap">
				<AmountInput bind:amount={savedPerMonth} bind:currency={savedPerMonthCurrency} />
			</div>

			<div class="question">
				{$t('earlyRetirementCalculator.interestRateSavings')}
			</div>
			<div class="er-calc-input-wrap">
				<div class="interest-rate-input-wrap">
					<input type="number" bind:value={savingInterestRate} step="0.1" min="0" max="20" />
					<span class="add-on">{$t('earlyRetirementCalculator.percentPerYear')}</span>
				</div>
			</div>

			<div class="buttons-wrap">
				<button type="button" on:click={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" on:click={goToPensionEligibility} class="er-calc-button">
					{$t('earlyRetirementCalculator.next')}
				</button>
			</div>
		</section>

		<section class="step" class:shown={currentSection === 'pension-eligibility'}>
			<div class="section-title">{$t('earlyRetirementCalculator.pension')}</div>

			<div class="question">{$t('earlyRetirementCalculator.eligibleForPension')}</div>
			<div class="er-calc-input-wrap">
				<Checkbox bind:value={eligibleForPension} />
			</div>

			<div class="buttons-wrap">
				<button type="button" on:click={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" on:click={goToPension} class="er-calc-button">
					{$t('earlyRetirementCalculator.next')}
				</button>
			</div>
		</section>

		<section class="step" class:shown={currentSection === 'pension'}>
			<div class="section-title">{$t('earlyRetirementCalculator.pensionDetails')}</div>

			<div class="question">
				{$t('earlyRetirementCalculator.pensionEligibilityAge')}
			</div>
			<div class="er-calc-input-wrap">
				<div class="interest-rate-input-wrap">
					<input type="number" bind:value={pensionAge} class:invalid={pensionAgeIsInvalid} min="50" max="99" />
					<span class="add-on">{$t('earlyRetirementCalculator.years')}</span>
				</div>
			</div>

			<div class="question">
				{$t('earlyRetirementCalculator.pensionIncome')}
			</div>
			<div class="er-calc-input-wrap">
				<AmountInput
					bind:amount={pensionPerMonth}
					bind:currency={pensionPerMonthCurrency}
					invalid={pensionPerMonthIsInvalid}
				/>
			</div>

			<div class="buttons-wrap">
				<button type="button" on:click={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" on:click={goToHaveLifeInsurance} class="er-calc-button">
					{$t('earlyRetirementCalculator.next')}
				</button>
			</div>
		</section>

		<section class="step" class:shown={currentSection === 'have-life-insurance'}>
			<div class="section-title">{$t('earlyRetirementCalculator.lifeInsurance')}</div>

			<div class="question">
				{$t('earlyRetirementCalculator.haveLifeInsurance')}
			</div>
			<div class="er-calc-input-wrap">
				<Checkbox bind:value={hasLifeInsurance} />
			</div>

			<div class="buttons-wrap">
				<button type="button" on:click={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" on:click={goToLifeInsurance} class="er-calc-button">
					{$t('earlyRetirementCalculator.next')}
				</button>
			</div>
		</section>

		<section class="step" class:shown={currentSection === 'life-insurance'}>
			<div class="section-title">{$t('earlyRetirementCalculator.lifeInsuranceDetails')}</div>

			<div class="question">
				{$t('earlyRetirementCalculator.lifeInsuranceAge')}
			</div>
			<div class="er-calc-input-wrap">
				<div class="interest-rate-input-wrap">
					<input
						type="number"
						bind:value={lifeInsuranceAge}
						class:invalid={lifeInsuranceAgeIsInvalid}
						min="30"
						max="99"
					/>
					<span class="add-on">{$t('earlyRetirementCalculator.years')}</span>
				</div>
			</div>

			<div class="question">
				{$t('earlyRetirementCalculator.lifeInsuranceReturn')}
			</div>
			<div class="er-calc-input-wrap">
				<AmountInput
					bind:amount={lifeInsuranceReturn}
					bind:currency={lifeInsuranceReturnCurrency}
					invalid={lifeInsuranceReturnIsInvalid}
				/>
			</div>

			<div class="buttons-wrap">
				<button type="button" on:click={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" on:click={goToUpcomingExpenses} class="er-calc-button">
					{$t('earlyRetirementCalculator.next')}
				</button>
			</div>
		</section>

		<section class="step" class:shown={currentSection === 'upcoming-expenses'}>
			<div class="section-title">{$t('earlyRetirementCalculator.upcomingExpenses')}</div>

			<div class="question">
				{$t('earlyRetirementCalculator.listAnyUpcomingExpenses')}
			</div>
			<div class="er-calc-input-wrap">
				<div class="large-upcoming-expenses">
					{#each upcomingExpenses as expense}
						<div class="large-upcoming-expense">
							<i class="large-upcoming-expense-icon {expense.iconClass}" />
							<input type="text" bind:value={expense.name} />
							<AmountInput bind:amount={expense.amount} bind:currency={expense.currency} />
							<button
								type="button"
								on:click={() => removeUpcomingExpense(expense)}
								class="remove-button"
								title={$t('earlyRetirementCalculator.removeExpense')}
								aria-label={$t('earlyRetirementCalculator.removeExpense')}
							>
								<i class="fas fa-times-circle" />
							</button>
						</div>
					{/each}

					<button
						type="button"
						on:click={addUpcomingExpense}
						class="add-button"
						title={$t('earlyRetirementCalculator.addExpense')}
						aria-label={$t('earlyRetirementCalculator.addExpense')}
					>
						<i class="fas fa-plus" />
					</button>
				</div>
			</div>

			<div class="buttons-wrap">
				<button type="button" on:click={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" on:click={goToRetirementIncome} class="er-calc-button">
					{$t('earlyRetirementCalculator.next')}
				</button>
			</div>
		</section>

		<section class="step" class:shown={currentSection === 'retirement-income'}>
			<div class="section-title">{$t('earlyRetirementCalculator.retirement')}</div>

			<div class="question">
				{$t('earlyRetirementCalculator.retirementIncome')}
			</div>
			<div class="er-calc-input-wrap">
				<AmountInput
					bind:amount={retirementIncome}
					bind:currency={retirementIncomeCurrency}
					invalid={preferredRetirementIncomeIsInvalid}
				/>
			</div>

			<div class="buttons-wrap">
				<button type="button" on:click={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" on:click={goToSummary} class="er-calc-button">
					{$t('earlyRetirementCalculator.next')}
				</button>
			</div>
		</section>

		<section class="step" class:shown={currentSection === 'summary'}>
			<div class="section-title">{$t('earlyRetirementCalculator.summary')}</div>

			{#if summaryItems}
				<ul>
					{#each summaryItems as summaryItem}
						<li>
							<span contenteditable="true" bind:innerHTML={summaryItem.contentHtml} />

							{#if summaryItem.children.length > 0}
								<ul>
									{#each summaryItem.children as childItem}
										<li contenteditable="true" bind:innerHTML={childItem.contentHtml} />
									{/each}
								</ul>
							{/if}
						</li>
					{/each}
				</ul>
			{/if}

			<div class="buttons-wrap">
				<button type="button" on:click={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" on:click={calculate} class="er-calc-button big">
					{$t('earlyRetirementCalculator.calculate')}
				</button>
			</div>
		</section>

		<section class="step" class:shown={currentSection === 'result'}>
			<div class="section-title">{$t('earlyRetirementCalculator.result')}</div>

			<div class="result-message">
				{#if earlyRetirementAge < 85}
					<span contenteditable="true" bind:innerHTML={consideringTheAnswersMessage} />
				{:else}
					<span>{$t('earlyRetirementCalculator.notLikelyToRetire')}</span>
				{/if}
			</div>

			<div class="buttons-wrap">
				<button type="button" on:click={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
			</div>
		</section>
	</div>
</section>

<style lang="scss">
	.er-calc-explanation {
		margin: 0 0 15px 0;
		font-size: 1.1rem;
		line-height: 28px;
		text-align: center;
	}

	.er-calc-highlight {
		color: var(--primary-color);
	}

	.er-calc-button {
		width: 100px;
		background: linear-gradient(225deg, #7a46f3, #00a6ed);
		background-size: 400% 400%;
		border: 1px solid transparent;
		border-radius: var(--border-radius);
		outline: none;
		padding: 6px 0;
		font-size: inherit;
		line-height: 27px;
		color: #fafafa;
		cursor: pointer;
		animation: AnimateGradiant 8s ease infinite;

		&.back {
			background: #fff;
			border-color: #ddd;
			color: var(--primary-color-dark);
		}

		&.big {
			width: 180px;
		}

		&.start {
			margin: 20px 0 30px;
		}
	}

	.buttons-wrap {
		display: flex;
		justify-content: space-between;
		margin-top: 3rem;
	}

	.step {
		display: none;

		&.shown {
			display: block;
		}
	}

	.section-title {
		border-bottom: 1px solid #ddd;
		padding-bottom: 12px;
		font-size: 1.25rem;
		text-align: center;
	}

	.question {
		margin: 35px 0 15px;
		text-align: center;
		line-height: 27px;

		&-detail {
			margin: 20px 0;
			text-align: center;
			line-height: 23px;
			color: var(--primary-color);
		}
	}

	.er-calc-input-wrap {
		text-align: center;

		.input-wrap {
			display: inline-flex;

			input {
				width: 60px;
			}
		}
	}

	.interest-rate-input-wrap {
		display: inline-flex;

		input {
			width: 60px;
			border-top-right-radius: 0;
			border-bottom-right-radius: 0;
			border-right: none;
			text-align: center;
		}

		.add-on {
			background: #ddd;
			border-radius: var(--border-radius);
			border-top-left-radius: 0;
			border-bottom-left-radius: 0;
			padding: 0 12px;
			line-height: 37px;
		}
	}

	.large-upcoming-expenses {
		margin-top: 15px;

		.large-upcoming-expense {
			display: flex;
			margin-top: 10px;

			i {
				flex-shrink: 0;
				width: 30px;
				text-align: center;
				font-size: 23px;
				line-height: 37px;
				color: var(--primary-color);

				&.fa-wallet {
					font-size: 21px;
				}
			}

			&-icon,
			input[type='text']:not(.select-currency-input),
			.input-wrap {
				margin-right: 7px;
			}

			.remove-button {
				background: transparent;
				border: none;
				outline: none;

				&:hover {
					color: var(--primary-color-dark);
				}
			}
		}

		.add-button {
			background: #fafafa;
			border: none;
			border-radius: 50%;
			box-shadow: var(--box-shadow);
			padding: 8px 15px;
			margin-top: 30px;
			line-height: 27px;
			color: var(--primary-color);

			&:hover {
				color: var(--primary-color-dark);
			}
		}
	}

	ul {
		padding: 0 25px;
		margin: 20px 0 15px;

		li {
			margin-bottom: 5px;
			line-height: 27px;

			ul {
				margin-top: 10px;
			}

			&:last-child {
				margin-bottom: 0;
			}
		}
	}

	.result-message {
		margin-top: 25px;
		line-height: 27px;
		text-align: center;
	}

	.retirement-age {
		color: var(--primary-color);
		font-size: 1.2rem;
		font-weight: bold;
	}

	@media screen and (min-width: 1200px) {
		.section-title {
			font-size: 1.4rem;
		}

		.question-detail {
			line-height: 27px;
		}

		.er-calc-input-wrap .input-wrap input {
			width: 100px;
		}

		.interest-rate-input-wrap .add-on {
			line-height: 45px;
		}

		.large-upcoming-expenses {
			.large-upcoming-expense i {
				width: 40px;
				font-size: 26px;
				line-height: 45px;
			}

			&-icon,
			input[type='text']:not(.select-currency-input),
			.input-wrap {
				margin-right: 10px;
			}
		}

		ul li {
			margin-bottom: 10px;
		}
	}
</style>
