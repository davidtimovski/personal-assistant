-- Configure auth

UPDATE "ClientRedirectUris" SET "RedirectUri" = 'http://localhost:8080/signin-oidc' WHERE "ClientId" = 1;
UPDATE "ClientRedirectUris" SET "RedirectUri" = 'http://localhost:8081/signin-oidc' WHERE "ClientId" = 2;
UPDATE "ClientRedirectUris" SET "RedirectUri" = 'http://localhost:8082/signin-oidc' WHERE "ClientId" = 3;

UPDATE "ClientPostLogoutRedirectUris" SET "PostLogoutRedirectUri" = 'http://localhost:8080' WHERE "ClientId" = 1;
UPDATE "ClientPostLogoutRedirectUris" SET "PostLogoutRedirectUri" = 'http://localhost:8081' WHERE "ClientId" = 2;
UPDATE "ClientPostLogoutRedirectUris" SET "PostLogoutRedirectUri" = 'http://localhost:8082' WHERE "ClientId" = 3;


-- Scramble user data

-- To Do Assistant
UPDATE "ToDoAssistant.Lists" SET "Name" = CONCAT('List ', CAST("Id" AS text)) WHERE "UserId" > 3;
UPDATE "ToDoAssistant.Tasks" SET "Name" = CONCAT('Task ', CAST("Id" AS text)) WHERE "ListId" IN (SELECT "Id" FROM "ToDoAssistant.Lists" WHERE "UserId" > 3);

-- Cooking Assistant
UPDATE "CookingAssistant.Recipes" SET "Name" = CONCAT('Recipe ', CAST("Id" AS text)) WHERE "UserId" > 3;
UPDATE "CookingAssistant.Ingredients" SET "Name" = CONCAT('Ingredient ', CAST("Id" AS text)) WHERE "UserId" > 3;

-- Accountant
UPDATE "Accountant.Accounts" SET "Name" = CONCAT('Account ', CAST("Id" AS text)) WHERE "UserId" > 3;
UPDATE "Accountant.Categories" SET "Name" = CONCAT('Category ', CAST("Id" AS text)) WHERE "UserId" > 3;
UPDATE "Accountant.Debts" SET "Person" = CONCAT('Debt ', CAST("Id" AS text)), "Description" = 'scrambled' WHERE "UserId" > 3;
UPDATE "Accountant.Transactions" SET "Description" = 'scrambled'
	WHERE "Description" IS NOT NULL AND (
		"FromAccountId" IN (SELECT "Id" FROM "Accountant.Accounts" WHERE "UserId" > 3) OR
		"ToAccountId" IN (SELECT "Id" FROM "Accountant.Accounts" WHERE "UserId" > 3)
	);
UPDATE "Accountant.UpcomingExpenses" SET "Description" = 'scrambled' WHERE "UserId" > 3 AND "Description" IS NOT NULL;
