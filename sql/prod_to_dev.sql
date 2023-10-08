DELETE FROM push_subscriptions;
UPDATE user_id_map SET auth0_id = 'auth0|6351a364595f24dcfed77fea' WHERE user_id = 2;

-- Scramble user data

-- To Do Assistant
UPDATE todo.lists SET name = CONCAT('List ', CAST(id AS text)) WHERE user_id > 3;
UPDATE todo.tasks SET name = CONCAT('Task ', CAST(id AS text)) WHERE list_id IN (SELECT id FROM todo.lists WHERE user_id > 3);

-- Chef
UPDATE chef.recipes SET name = CONCAT('Recipe ', CAST(id AS text)) WHERE user_id > 3;
UPDATE chef.ingredients SET name = CONCAT('Ingredient ', CAST(id AS text)) WHERE user_id > 3;

-- Accountant
UPDATE accountant.accounts SET name = CONCAT('Account ', CAST(id AS text)) WHERE user_id > 3;
UPDATE accountant.categories SET name = CONCAT('Category ', CAST(id AS text)) WHERE user_id > 3;
UPDATE accountant.debts SET person = CONCAT('Debt ', CAST(id AS text)), description = 'scrambled' WHERE user_id > 3;
UPDATE accountant.transactions SET amount = floor(random() * amount + 1), description = 'scrambled'
    WHERE description IS NOT NULL AND (
        from_account_id IN (SELECT id FROM accountant.accounts WHERE user_id > 3) OR
        to_account_id IN (SELECT id FROM accountant.accounts WHERE user_id > 3)
    );
UPDATE accountant.upcoming_expenses SET description = 'scrambled' WHERE user_id > 3 AND description IS NOT NULL;
UPDATE accountant.automatic_transactions SET description = 'scrambled' WHERE user_id > 3 AND description IS NOT NULL;
