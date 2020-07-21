-- Table: public."Tooltips"

-- DROP TABLE public."Tooltips";

CREATE TABLE public."Tooltips"
(
    "Id" serial NOT NULL,
	"Application" character varying(20) NOT NULL COLLATE pg_catalog."default",
    "Key" character varying(30) NOT NULL COLLATE pg_catalog."default",
    CONSTRAINT "PK_Tooltips" PRIMARY KEY ("Id")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."Tooltips"
    OWNER to personalassistant;

-- Index: IX_Tooltips_Key

-- DROP INDEX public."IX_Tooltips_Key";

CREATE INDEX "IX_Tooltips_Key"
    ON public."Tooltips" USING btree
    ("Key")
    TABLESPACE pg_default;