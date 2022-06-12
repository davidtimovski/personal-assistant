CREATE TABLE public.tooltips_dismissed
(
    tooltip_id integer NOT NULL,
    user_id integer NOT NULL,
    CONSTRAINT fk_tooltips_dismissed_aspnetusers_user_id FOREIGN KEY (user_id)
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    UNIQUE (tooltip_id, user_id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.tooltips_dismissed
    OWNER to personalassistant;

CREATE INDEX ix_tooltips_dismissed_tooltip_id
    ON public.tooltips_dismissed USING btree
    (tooltip_id)
    TABLESPACE pg_default;

CREATE INDEX ix_tooltips_dismissed_user_id
    ON public.tooltips_dismissed USING btree
    (user_id)
    TABLESPACE pg_default;
