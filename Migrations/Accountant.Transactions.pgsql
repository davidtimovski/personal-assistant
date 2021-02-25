-- Table: public."Accountant.Transactions"

-- DROP TABLE public."Accountant.Transactions";

CREATE TABLE public."Accountant.Transactions"
(
    "Id" serial NOT NULL,
	"FromAccountId" integer,
	"ToAccountId" integer,
    "CategoryId" integer,
	"Amount" decimal(10, 2) NOT NULL,
	"FromStocks" decimal(10, 4),
	"ToStocks" decimal(10, 4),
	"Currency" character varying(3) COLLATE pg_catalog."default" NOT NULL DEFAULT 'EUR',
    "Description" character varying(500) COLLATE pg_catalog."default",
	"Date" date NOT NULL,
	"IsEncrypted" boolean NOT NULL DEFAULT FALSE,
	"EncryptedDescription" bytea,
	"Salt" bytea,
	"Nonce" bytea,
	"EncryptionHint" character varying(100) COLLATE pg_catalog."default",
    "CreatedDate" timestamp without time zone NOT NULL,
    "ModifiedDate" timestamp without time zone NOT NULL,
    CONSTRAINT "FK_Accountant.Transactions_Accountant.Accounts_ToAccountId" FOREIGN KEY ("ToAccountId")
    REFERENCES public."Accountant.Accounts" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
	CONSTRAINT "FK_Accountant.Transactions_Accountant.Accounts_FromAccountId" FOREIGN KEY ("FromAccountId")
    REFERENCES public."Accountant.Accounts" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT "FK_Accountant.Transactions_Accountant.Categories_CategoryId" FOREIGN KEY ("CategoryId")
    REFERENCES public."Accountant.Categories" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."Accountant.Transactions"
    OWNER to personalassistant;

-- Index: IX_Accountant.Transactions_UserId

-- DROP INDEX public."IX_Accountant.Transactions_UserId";

CREATE INDEX "IX_Accountant.Transactions_UserId"
    ON public."Accountant.Transactions" USING btree
    ("UserId")
    TABLESPACE pg_default;
	
-- Index: IX_Accountant.Transactions_FromAccountId

-- DROP INDEX public."IX_Accountant.Transactions_FromAccountId";

CREATE INDEX "IX_Accountant.Transactions_FromAccountId"
    ON public."Accountant.Transactions" USING btree
    ("FromAccountId")
    TABLESPACE pg_default;
	
-- Index: IX_Accountant.Transactions_ToAccountId
	
-- DROP INDEX public."IX_Accountant.Transactions_ToAccountId";

CREATE INDEX "IX_Accountant.Transactions_ToAccountId"
    ON public."Accountant.Transactions" USING btree
    ("ToAccountId")
    TABLESPACE pg_default;
	
-- Index: IX_Accountant.Transactions_CategoryId

-- DROP INDEX public."IX_Accountant.Transactions_CategoryId";

CREATE INDEX "IX_Accountant.Transactions_CategoryId"
    ON public."Accountant.Transactions" USING btree
    ("CategoryId")
    TABLESPACE pg_default;
