using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Control.Infrastructure.Migrations;

public partial class AddActualizarEstadoProcedure : Migration{
    protected override void Up(MigrationBuilder migrationBuilder){
        migrationBuilder.Sql("""
            CREATE OR REPLACE PROCEDURE sp_actualizar_estado_carga(
                p_id uuid,
                p_estado varchar(30)
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                UPDATE "CargasArchivo"
                SET
                    "Estado" = p_estado,
                    "FechaFin" = CASE
                        WHEN p_estado = 'Finalizado' THEN NOW()
                        ELSE "FechaFin"
                    END
                WHERE "Id" = p_id;
            END;
            $$;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder){
        migrationBuilder.Sql("""DROP PROCEDURE IF EXISTS sp_actualizar_estado_carga(uuid, varchar);""");
    }
}