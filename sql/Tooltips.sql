CREATE TABLE public.tooltips
(
    id serial NOT NULL,
    application character varying(20) NOT NULL COLLATE pg_catalog."default",
    key character varying(30) NOT NULL COLLATE pg_catalog."default",
    CONSTRAINT pk_tooltips PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.tooltips
    OWNER to personalassistant;

CREATE INDEX ix_tooltips_key
    ON public.tooltips USING btree
    (key)
    TABLESPACE pg_default;
    
CREATE INDEX ix_tooltips_application
    ON public.tooltips USING btree
    (application)
    TABLESPACE pg_default;
