-- Configure auth

UPDATE "ClientRedirectUris" SET "RedirectUri" = 'http://localhost:8080/signin-oidc' WHERE "ClientId" = 1;
UPDATE "ClientRedirectUris" SET "RedirectUri" = 'http://localhost:8081/signin-oidc' WHERE "ClientId" = 2;
UPDATE "ClientRedirectUris" SET "RedirectUri" = 'http://localhost:8082/signin-oidc' WHERE "ClientId" = 3;

UPDATE "ClientPostLogoutRedirectUris" SET "PostLogoutRedirectUri" = 'http://localhost:8080' WHERE "ClientId" = 1;
UPDATE "ClientPostLogoutRedirectUris" SET "PostLogoutRedirectUri" = 'http://localhost:8081' WHERE "ClientId" = 2;
UPDATE "ClientPostLogoutRedirectUris" SET "PostLogoutRedirectUri" = 'http://localhost:8082' WHERE "ClientId" = 3;


-- Scramble user data

-- To Do Assistant
UPDATE todo_lists SET name = CONCAT('List ', CAST(id AS text)) WHERE user_id > 3;
UPDATE todo_tasks SET name = CONCAT('Task ', CAST(id AS text)) WHERE list_id IN (SELECT id FROM todo_lists WHERE user_id > 3);

-- Cooking Assistant
UPDATE cooking_recipes SET name = CONCAT('Recipe ', CAST(id AS text)) WHERE user_id > 3;
UPDATE cooking_ingredients SET name = CONCAT('Ingredient ', CAST(id AS text)) WHERE user_id > 3;

-- Accountant
UPDATE accountant_accounts SET name = CONCAT('Account ', CAST(id AS text)) WHERE user_id > 3;
UPDATE accountant_categories SET name = CONCAT('Category ', CAST(id AS text)) WHERE user_id > 3;
UPDATE accountant_debts SET person = CONCAT('Debt ', CAST(id AS text)), description = 'scrambled' WHERE user_id > 3;
UPDATE accountant_transactions SET description = 'scrambled'
	WHERE description IS NOT NULL AND (
		from_account_id IN (SELECT id FROM accountant_accounts WHERE user_id > 3) OR
		to_account_id IN (SELECT id FROM accountant_accounts WHERE user_id > 3)
	);
UPDATE accountant_upcoming_expenses SET description = 'scrambled' WHERE user_id > 3 AND description IS NOT NULL;
