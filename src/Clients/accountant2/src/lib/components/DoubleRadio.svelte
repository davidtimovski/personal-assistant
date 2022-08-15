<script lang="ts">
	import { t } from '$lib/localization/i18n';

	export let name: string;
	export let leftLabelKey: string;
	export let rightLabelKey: string;
	export let value: boolean;

	let leftLabel = $t(leftLabelKey);
	let rightLabel = $t(rightLabelKey);
</script>

<div class="double-radio-wrap">
	<div class="double-radio-side">
		<label class:selected={!value}>
			<span>{leftLabel}</span>
			<input type="radio" {name} bind:group={value} value={false} />
			<span class="checkbox-icon">
				<i class="fas fa-check" />
			</span>
		</label>
	</div>
	<div class="double-radio-side">
		<label class:selected={value}>
			<span>{rightLabel}</span>
			<input type="radio" {name} bind:group={value} value={true} />
			<span class="checkbox-icon">
				<i class="fas fa-check" />
			</span>
		</label>
	</div>
</div>

<style lang="scss">
	.double-radio-wrap {
		display: flex;
		border: 1px solid #ddd;
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
			color: #fff;

			transition: color var(--transition);
		}

		.double-radio-side {
			width: 50%;

			label {
				display: block;
				position: relative;
				background: #fff;
				padding: 0 48px 0 15px;
				font-size: inherit;
				line-height: 37px;
				cursor: pointer;
				user-select: none;

				transition: background var(--transition), color var(--transition);

				&.selected {
					background: var(--primary-color);
					color: #fff;

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
</style>
