-- Table: public."PushSubscriptions"

-- DROP TABLE public."PushSubscriptions";

CREATE TABLE public."PushSubscriptions"
(
    "Id" serial NOT NULL,
    "UserId" integer NOT NULL,
    "Application" character varying(50) NOT NULL COLLATE pg_catalog."default",
    "Endpoint" character varying(255) NOT NULL COLLATE pg_catalog."default",
    "AuthKey" character varying(255) NOT NULL COLLATE pg_catalog."default",
    "P256dhKey" character varying(255) NOT NULL COLLATE pg_catalog."default",
    "CreatedDate" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_PushSubscriptions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PushSubscriptions_AspNetUsers_UserId" FOREIGN KEY ("UserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."PushSubscriptions"
    OWNER to personalassistant;

-- Index: IX_PushSubscriptions_UserId

-- DROP INDEX public."IX_PushSubscriptions_UserId";

CREATE INDEX "IX_PushSubscriptions_UserId"
    ON public."PushSubscriptions" USING btree
    ("UserId")
    TABLESPACE pg_default;