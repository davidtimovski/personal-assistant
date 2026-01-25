<script lang="ts">
	import { onMount, onDestroy } from 'svelte';

	import { CurrenciesService } from '../../../../../../Core/shared2/services/currenciesService';
	import Checkbox from '../../../../../../Core/shared2/components/Checkbox.svelte';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { Formatter } from '$lib/utils/formatter';
	import { user } from '$lib/stores';
	import { AccountsService } from '$lib/services/accountsService';
	import { LargeUpcomingExpense, SummaryItem } from '$lib/models/viewmodels/earlyRetirementCalculator';
	import { YearSummaryItem } from './viewmodels/yearSummaryItem';

	import AmountInput from '$lib/components/AmountInput.svelte';

	// Answers
	let age: number | null = $state(null);
	let capital: number | null = $state(null);
	let capitalCurrency: string | null = $state(null);
	let savedPerMonth: number | null = $state(null);
	let savedPerMonthCurrency: string | null = $state(null);
	let savingInterestRate: number | null = $state(null);
	let eligibleForPension = $state(false);
	let pensionAge: number | null = $state(null);
	let pensionPerMonth: number | null = $state(null);
	let pensionPerMonthCurrency: string | null = $state(null);
	let hasLifeInsurance = $state(false);
	let lifeInsuranceAge: number | null = $state(null);
	let lifeInsuranceReturn: number | null = $state(null);
	let lifeInsuranceReturnCurrency: string | null = $state(null);
	let upcomingExpenses: LargeUpcomingExpense[] = $state([]);
	let retirementIncome: number | null = $state(null);
	let retirementIncomeCurrency: string | null = $state(null);
	let consideringTheAnswersMessage: string | null = $state(null);

	let currentSection = $state('start');
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
	let currency: string | null = $state(null);
	let ageIsInvalid = $state(false);
	let pensionAgeIsInvalid = $state(false);
	let pensionPerMonthIsInvalid = $state(false);
	let lifeInsuranceAgeIsInvalid = $state(false);
	let lifeInsuranceReturnIsInvalid = $state(false);
	let preferredRetirementIncomeIsInvalid = $state(false);
	let earlyRetirementAge: number | null = $state(null);
	let summaryItems: SummaryItem[] = $state([]);
	let yearlySummaryItems: YearSummaryItem[] = $state([]);
	const lifespan = 85;
	const yearlyInflation = 0.025;

	const localStorage = new LocalStorageUtil();
	let accountsService: AccountsService;
	let currenciesService: CurrenciesService;

	function addUpcomingExpense() {
		if (currency === null) {
			throw new Error('Currency not initialized yet');
		}

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
						capital: Formatter.moneyPrecise(capital, capitalCurrency, $user.culture, 0)
					})
				)
			);
		} else {
			summaryItems.push(new SummaryItem($t('earlyRetirementCalculator.summaryItem2b')));
		}

		if (savedPerMonth && parseFloat(savedPerMonth.toString()) > 0) {
			if (savingInterestRate) {
				summaryItems.push(
					new SummaryItem(
						$t('earlyRetirementCalculator.summaryItem3a', {
							savedPerMonth: Formatter.moneyPrecise(savedPerMonth, savedPerMonthCurrency, $user.culture, 0),
							savingInterestRate: savingInterestRate
						})
					)
				);
			} else {
				summaryItems.push(
					new SummaryItem(
						$t('earlyRetirementCalculator.summaryItem3b', {
							savedPerMonth: Formatter.moneyPrecise(savedPerMonth, savedPerMonthCurrency, $user.culture, 0)
						})
					)
				);
			}
		} else {
			summaryItems.push(new SummaryItem($t('earlyRetirementCalculator.summaryItem3c')));
		}

		if (eligibleForPension) {
			summaryItems.push(
				new SummaryItem(
					$t('earlyRetirementCalculator.summaryItem4a', {
						pensionAge: pensionAge,
						pensionPerMonth: Formatter.moneyPrecise(pensionPerMonth, pensionPerMonthCurrency, $user.culture, 0)
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
						lifeInsuranceReturn: Formatter.moneyPrecise(lifeInsuranceReturn, lifeInsuranceReturnCurrency, $user.culture, 0),
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
							amount: Formatter.moneyPrecise(expense.amount, expense.currency, $user.culture, 0),
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
					retirementIncome: Formatter.moneyPrecise(retirementIncome, retirementIncomeCurrency, $user.culture, 0)
				})
			)
		);

		summaryItems.push(new SummaryItem($t('earlyRetirementCalculator.summaryItem8', { lifespan: lifespan, inflation: yearlyInflation * 100 })));

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
		if (age === null) {
			throw new Error('Age is not set');
		}

		const largeUpcomingExpensesSum = upcomingExpenses
			.map((x) => getConvertedValueOrZero(<number>x.amount, x.currency))
			.reduce((a: number, b: number) => a + b, 0);

		let pensionSum = 0;
		if (eligibleForPension) {
			const yearsTakingPension = lifespan - getIntValueOrZero(pensionAge);
			const pension = getConvertedValueOrZero(<number>pensionPerMonth, pensionPerMonthCurrency);
			pensionSum = yearsTakingPension * 12 * pension;
		}

		lifeInsuranceAge = getIntValueOrZero(lifeInsuranceAge);
		const lifeInsuranceConverted = hasLifeInsurance ? getConvertedValueOrZero(<number>lifeInsuranceReturn, lifeInsuranceReturnCurrency) : 0;

		const savingInterestMonthlyFactor = 1 + getFloatValueOrZero(savingInterestRate) / 100 / 12;
		const savedPerMonthConverted = getConvertedValueOrZero(<number>savedPerMonth, savedPerMonthCurrency);

		let ageInYear = parseInt(age.toString(), 10);
		const currentAge = ageInYear;
		let saved = getConvertedValueOrZero(<number>capital, capitalCurrency);

		let requiredCapitalToRetireAtAge = sumRequiredCapital(currentAge, ageInYear, largeUpcomingExpensesSum, pensionSum);

		let currentYear = new Date().getFullYear();
		let yearlySummaryItemsTemp: YearSummaryItem[] = [];
		let totalInterest = 0;

		while (true) {
			yearlySummaryItemsTemp.push(
				new YearSummaryItem(currentYear, ageInYear, Math.floor(totalInterest), Math.floor(requiredCapitalToRetireAtAge), Math.floor(saved))
			);

			if (saved >= requiredCapitalToRetireAtAge) {
				break;
			}

			// Apply savings and interest for year
			for (let i = 0; i < 12; i++) {
				saved *= savingInterestMonthlyFactor;
				saved += savedPerMonthConverted;

				if (savingInterestRate !== null) {
					totalInterest += saved * savingInterestMonthlyFactor - saved;
				}
			}

			// Apply any insurance payments that may occur in the year
			if (hasLifeInsurance && ageInYear === lifeInsuranceAge) {
				saved += lifeInsuranceConverted;
			}

			currentYear++;
			ageInYear++;

			if (ageInYear === lifespan) {
				break;
			}

			requiredCapitalToRetireAtAge = sumRequiredCapital(currentAge, ageInYear, largeUpcomingExpensesSum, pensionSum);
		}

		yearlySummaryItems = yearlySummaryItemsTemp;

		earlyRetirementAge = ageInYear;

		if (earlyRetirementAge < lifespan) {
			consideringTheAnswersMessage = $t('earlyRetirementCalculator.consideringTheAnswers', {
				earlyRetirementAge: earlyRetirementAge,
				requiredCapitalToRetireAtAge: Formatter.moneyPrecise(Math.floor(requiredCapitalToRetireAtAge), currency, $user.culture, 0)
			});
		}

		const currentIndex = sections.indexOf(currentSection);
		currentSection = sections[currentIndex + 1];
	}

	function sumRequiredCapital(currentAge: number, ageInYear: number, largeUpcomingExpensesSum: number, pensionSum: number) {
		const monthlyInflationFactor = 1 + yearlyInflation / 12;

		let retirementIncomeConverted = getConvertedValueOrZero(<number>retirementIncome, retirementIncomeCurrency);
		let retirementAmountNeeded = 0;

		// Loop from current age and inflate desired retirement income
		// Once the retirement year occurs, start summing the required retirement income amount
		for (let age = currentAge; age < lifespan; age++) {
			for (let month = 0; month < 12; month++) {
				retirementIncomeConverted = retirementIncomeConverted * monthlyInflationFactor;

				if (age >= ageInYear) {
					retirementAmountNeeded += retirementIncomeConverted;
				}
			}
		}

		return retirementAmountNeeded + largeUpcomingExpensesSum - pensionSum;
	}

	function getIntValueOrZero(value: number | null): number {
		if (!value || value.toString().trim() === '') {
			return 0;
		}

		return parseInt(value.toString(), 10);
	}

	function getFloatValueOrZero(value: number | null): number {
		if (!value || value.toString().trim() === '') {
			return 0;
		}

		return parseFloat(value.toString());
	}

	function getConvertedValueOrZero(amount: number | null, amountCurrency: string | null): number {
		if (currency === null) {
			throw new Error('Currency not initialized yet');
		}

		if (!amount || !amountCurrency || amount.toString().trim() === '') {
			return 0;
		}

		const parsed = parseFloat(amount.toString());

		return currenciesService.convert(parsed, amountCurrency, currency);
	}

	onMount(async () => {
		accountsService = new AccountsService();
		currenciesService = new CurrenciesService('Accountant');

		currency = localStorage.get(LocalStorageKeys.Currency);

		capitalCurrency = savedPerMonthCurrency = pensionPerMonthCurrency = lifeInsuranceReturnCurrency = retirementIncomeCurrency = currency;

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
			<i class="fas fa-piggy-bank"></i>
		</div>
		<div class="page-title">{$t('earlyRetirementCalculator.earlyRetirementCalculator')}</div>
		<a href="/dashboard" class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="content-wrap">
		<section class="step" class:shown={currentSection === 'start'}>
			<p class="er-calc-explanation">{$t('earlyRetirementCalculator.explanation')}</p>

			<div class="centering-wrap">
				<button type="button" onclick={start} class="er-calc-button big start">{$t('earlyRetirementCalculator.start')}</button>
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
				<button type="button" onclick={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" onclick={goToCapital} class="er-calc-button">
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
				<button type="button" onclick={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" onclick={goToSaving} class="er-calc-button">
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
				<button type="button" onclick={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" onclick={goToPensionEligibility} class="er-calc-button">
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
				<button type="button" onclick={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" onclick={goToPension} class="er-calc-button">
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
				<AmountInput bind:amount={pensionPerMonth} bind:currency={pensionPerMonthCurrency} invalid={pensionPerMonthIsInvalid} />
			</div>

			<div class="buttons-wrap">
				<button type="button" onclick={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" onclick={goToHaveLifeInsurance} class="er-calc-button">
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
				<button type="button" onclick={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" onclick={goToLifeInsurance} class="er-calc-button">
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
					<input type="number" bind:value={lifeInsuranceAge} class:invalid={lifeInsuranceAgeIsInvalid} min="30" max="99" />
					<span class="add-on">{$t('earlyRetirementCalculator.years')}</span>
				</div>
			</div>

			<div class="question">
				{$t('earlyRetirementCalculator.lifeInsuranceReturn')}
			</div>
			<div class="er-calc-input-wrap">
				<AmountInput bind:amount={lifeInsuranceReturn} bind:currency={lifeInsuranceReturnCurrency} invalid={lifeInsuranceReturnIsInvalid} />
			</div>

			<div class="buttons-wrap">
				<button type="button" onclick={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" onclick={goToUpcomingExpenses} class="er-calc-button">
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
							<i class="large-upcoming-expense-icon {expense.iconClass}"></i>
							<input type="text" bind:value={expense.name} />
							<AmountInput bind:amount={expense.amount} bind:currency={expense.currency} />
							<button
								type="button"
								onclick={() => removeUpcomingExpense(expense)}
								class="remove-button"
								title={$t('earlyRetirementCalculator.removeExpense')}
								aria-label={$t('earlyRetirementCalculator.removeExpense')}
							>
								<i class="fas fa-times-circle"></i>
							</button>
						</div>
					{/each}

					<button
						type="button"
						onclick={addUpcomingExpense}
						class="add-button"
						title={$t('earlyRetirementCalculator.addExpense')}
						aria-label={$t('earlyRetirementCalculator.addExpense')}
					>
						<i class="fas fa-plus"></i>
					</button>
				</div>
			</div>

			<div class="buttons-wrap">
				<button type="button" onclick={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" onclick={goToRetirementIncome} class="er-calc-button">
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
				<AmountInput bind:amount={retirementIncome} bind:currency={retirementIncomeCurrency} invalid={preferredRetirementIncomeIsInvalid} />
			</div>

			<div class="buttons-wrap">
				<button type="button" onclick={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" onclick={goToSummary} class="er-calc-button">
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
							<span contenteditable="false" bind:innerHTML={summaryItem.contentHtml}></span>

							{#if summaryItem.children.length > 0}
								<ul>
									{#each summaryItem.children as childItem}
										<li contenteditable="false" bind:innerHTML={childItem.contentHtml}></li>
									{/each}
								</ul>
							{/if}
						</li>
					{/each}
				</ul>
			{/if}

			<div class="buttons-wrap">
				<button type="button" onclick={back} class="er-calc-button back">
					{$t('earlyRetirementCalculator.back')}
				</button>
				<button type="button" onclick={calculate} class="er-calc-button big">
					{$t('earlyRetirementCalculator.calculate')}
				</button>
			</div>
		</section>

		<section class="step" class:shown={currentSection === 'result'}>
			<div class="section-title">{$t('earlyRetirementCalculator.result')}</div>

			{#if earlyRetirementAge}
				<div class="result-message">
					{#if earlyRetirementAge < lifespan}
						<span contenteditable="false" bind:innerHTML={consideringTheAnswersMessage}></span>

						<table class="summary-table">
							<thead>
								<tr>
									<th>{$t('earlyRetirementCalculator.year')}</th>
									<th>{$t('earlyRetirementCalculator.age')}</th>
									{#if savingInterestRate}
										<th>{$t('earlyRetirementCalculator.interest', { savingInterestRate: savingInterestRate })}</th>
									{/if}
									<th>{$t('earlyRetirementCalculator.required')}</th>
									<th>{$t('earlyRetirementCalculator.saved')}</th>
								</tr>
							</thead>
							<tbody>
								{#each yearlySummaryItems as savedSummaryItem}
									<tr>
										<td>{savedSummaryItem.year}</td>
										<td>{savedSummaryItem.age}</td>
										{#if savingInterestRate}
											<td>{Formatter.moneyPrecise(savedSummaryItem.totalInterest, currency, $user.culture, 0)}</td>
										{/if}
										<td>{Formatter.moneyPrecise(savedSummaryItem.required, currency, $user.culture, 0)}</td>
										<td>{Formatter.moneyPrecise(savedSummaryItem.totalSaved, currency, $user.culture, 0)}</td>
									</tr>
								{/each}
							</tbody>
						</table>
					{:else}
						<span>{$t('earlyRetirementCalculator.notLikelyToRetire')}</span>
					{/if}
				</div>
			{/if}

			<div class="buttons-wrap">
				<button type="button" onclick={back} class="er-calc-button back">
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

	.summary-table {
		width: 100%;
		margin-top: 40px;

		th,
		td {
			text-align: right;
		}
		th:nth-child(-n + 2),
		td:nth-child(-n + 2) {
			text-align: left;
		}

		th {
			border-bottom: 1px solid #ddd;
		}
	}

	@media screen and (min-width: 1200px) {
		.section-title {
			font-size: 1.4rem;
		}

		.question-detail {
			line-height: 27px;
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
