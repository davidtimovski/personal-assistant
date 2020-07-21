-- Table: public."TooltipsDismissed"

-- DROP TABLE public."TooltipsDismissed";

CREATE TABLE public."TooltipsDismissed"
(
    "TooltipId" integer NOT NULL,
    "UserId" integer NOT NULL,
    CONSTRAINT "FK_TooltipsDismissed_AspNetUsers_UserId" FOREIGN KEY ("UserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    UNIQUE ("TooltipId", "UserId")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."TooltipsDismissed"
    OWNER to personalassistant;

-- Index: IX_TooltipsDismissed_TooltipId

-- DROP INDEX public."IX_TooltipsDismissed_TooltipId";

CREATE INDEX "IX_TooltipsDismissed_TooltipId"
    ON public."TooltipsDismissed" USING btree
    ("TooltipId")
    TABLESPACE pg_default;

-- Index: IX_TooltipsDismissed_UserId

-- DROP INDEX public."IX_TooltipsDismissed_UserId";

CREATE INDEX "IX_TooltipsDismissed_UserId"
    ON public."TooltipsDismissed" USING btree
    ("UserId")
    TABLESPACE pg_default;