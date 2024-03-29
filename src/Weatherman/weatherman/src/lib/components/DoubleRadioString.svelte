<script lang="ts">
	import { createEventDispatcher } from 'svelte';
	import { t } from '$lib/localization/i18n';

	export let name: string;
	export let leftLabelKey: string;
	export let rightLabelKey: string;
	export let leftValue: string;
	export let rightValue: string;
	export let value: string | null;

	let leftLabel = $t(leftLabelKey);
	let rightLabel = $t(rightLabelKey);

	const dispatch = createEventDispatcher();
	function changeState() {
		dispatch('change', value);
	}
</script>

<div class="double-radio-wrap">
	<div class="double-radio-side">
		<label class:selected={value === leftValue}>
			<span>{leftLabel}</span>
			<input type="radio" {name} bind:group={value} value={leftValue} on:change={changeState} />
			<span class="checkbox-icon">
				<i class="fas fa-check" />
			</span>
		</label>
	</div>
	<div class="double-radio-side">
		<label class:selected={value === rightValue}>
			<span>{rightLabel}</span>
			<input type="radio" {name} bind:group={value} value={rightValue} on:change={changeState} />
			<span class="checkbox-icon">
				<i class="fas fa-check" />
			</span>
		</label>
	</div>
</div>

<style lang="scss">
	.double-radio-wrap {
		display: flex;
		border: 1px solid;
		border-radius: 8px;

		input[type='radio'] {
			display: none;
		}

		.checkbox-icon {
			display: none;
			position: absolute;
			top: 5px;
			right: 12px;
			font-size: 21px;

			transition: color var(--transition);
		}

		.double-radio-side {
			width: 50%;

			label {
				display: block;
				position: relative;
				padding: 0 48px 0 15px;
				font-size: inherit;
				line-height: 37px;
				cursor: pointer;
				user-select: none;

				transition: background var(--transition), color var(--transition);

				&.selected {
					background: var(--primary-color);

					.checkbox-icon {
						display: inline;
					}
				}
			}

			&:nth-child(1) label {
				border-top-left-radius: 8px;
				border-bottom-left-radius: 8px;
			}

			&:nth-child(2) label {
				border-top-right-radius: 8px;
				border-bottom-right-radius: 8px;
			}

			span {
				line-height: 27px;
			}
		}
	}

	/* Desktop */
	@media screen and (min-width: 700px) {
		.double-radio-wrap .double-radio-side label {
			padding: 4px 48px 4px 15px;
		}
		.double-radio-wrap .checkbox-icon {
			top: 9px;
			font-size: 23px;
		}
	}

	@media (prefers-color-scheme: light) {
		.double-radio-wrap {
			border-color: #ddd;

			.checkbox-icon {
				color: #fff;
			}

			.double-radio-side {
				label {
					background: #fff;

					&.selected {
						color: #fff;
					}
				}
			}
		}
	}

	@media (prefers-color-scheme: dark) {
		.double-radio-wrap {
			border-color: #666;

			.checkbox-icon {
				color: #fff;
			}

			.double-radio-side {
				label {
					background: #333;

					&.selected {
						color: #fff;
					}
				}
			}
		}
	}
</style>
