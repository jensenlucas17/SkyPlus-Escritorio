USE skyplus;

SET NAMES utf8mb4;
SET time_zone = '-03:00';

-- Fecha de referencia: hoy en Argentina.
SET @fecha_base = CURDATE();

SET @ahora_utc = UTC_TIMESTAMP();
SET @ahora_local = NOW();

-- Contraseña de TODOS los usuarios demo: SkyPlusDemo2026!
-- Hash compatible con PasswordService.
SET @hash_demo =
'PBKDF2$600000$jFApta/EFW/g/OCXlDWahA==$uAznqpfAxSN54hbcuE45CYc6no0UVb9nZEB4OlKmOB4=';

DELIMITER $$

BEGIN NOT ATOMIC

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    DROP TEMPORARY TABLE IF EXISTS demo_plan;

    CREATE TEMPORARY TABLE demo_plan (
        id INT PRIMARY KEY,
        dia INT,
        ruta INT,
        objetivo INT
    ) ENGINE=InnoDB;

    START TRANSACTION;

    -- ==========================================
    -- 1. LIMPIEZA
    -- Las claves foráneas permanecen activas.
    -- ==========================================

    DELETE FROM pago_detalle;
    DELETE FROM reembolso;
    DELETE FROM checkin;
    DELETE FROM cargo_adicional;
    DELETE FROM equipaje_reserva;
    DELETE FROM pago;
    DELETE FROM reserva;
    DELETE FROM asiento;
    DELETE FROM vuelo;
    DELETE FROM usuario;
    DELETE FROM rol;
    DELETE FROM pasajero;
    DELETE FROM aeronave;
    DELETE FROM lugar;
    DELETE FROM tipo_equipaje;

    -- NO borrar __efmigrationshistory.

    -- ==========================================
    -- 2. ROLES Y USUARIOS
    -- ==========================================

    INSERT INTO rol (
        id_rol,
        nombre_rol,
        estado
    )
    VALUES
        (1, 'Administrador', 'Activo'),
        (2, 'Gerente', 'Activo'),
        (3, 'Agente de Reservas', 'Activo'),
        (4, 'Agente de Check-in', 'Activo');

    INSERT INTO usuario (
        id_usuario,
        id_rol,
        nombre,
        apellido,
        email_corporativo,
        contrasena_hash,
        estado_cuenta,
        dni
    )
    VALUES
        (
            1, 1, 'Lucia', 'Demo',
            'admin@skyplus.test',
            @hash_demo, 'Activo', '30000001'
        ),
        (
            2, 1, 'Mateo', 'Demo',
            'admin2@skyplus.test',
            @hash_demo, 'Activo', '30000002'
        ),
        (
            3, 2, 'Valentina', 'Demo',
            'gerente@skyplus.test',
            @hash_demo, 'Activo', '30000003'
        ),
        (
            4, 3, 'Sofia', 'Demo',
            'reservas@skyplus.test',
            @hash_demo, 'Activo', '30000004'
        ),
        (
            5, 3, 'Tomas', 'Demo',
            'reservas2@skyplus.test',
            @hash_demo, 'Activo', '30000005'
        ),
        (
            6, 4, 'Camila', 'Demo',
            'checkin@skyplus.test',
            @hash_demo, 'Activo', '30000006'
        ),
        (
            7, 3, 'Bruno', 'Demo',
            'inactivo@skyplus.test',
            @hash_demo, 'Inactivo', '30000007'
        );

    -- ==========================================
    -- 3. AEROPUERTOS
    -- ==========================================

    INSERT INTO lugar (
        id_lugar,
        codigo_iata,
        nombre,
        ciudad,
        pais,
        estado
    )
    VALUES
        (
            1, 'AEP', 'Aeroparque Jorge Newbery',
            'Buenos Aires', 'Argentina', 'Activo'
        ),
        (
            2, 'COR', 'Ingeniero Ambrosio Taravella',
            'Córdoba', 'Argentina', 'Activo'
        ),
        (
            3, 'MDZ', 'El Plumerillo',
            'Mendoza', 'Argentina', 'Activo'
        ),
        (
            4, 'BRC', 'Teniente Luis Candelaria',
            'Bariloche', 'Argentina', 'Activo'
        ),
        (
            5, 'EZE', 'Ministro Pistarini',
            'Ezeiza', 'Argentina', 'Activo'
        ),
        (
            6, 'IGR', 'Cataratas del Iguazú',
            'Puerto Iguazú', 'Argentina', 'Activo'
        ),
        (
            7, 'SLA', 'Martín Miguel de Güemes',
            'Salta', 'Argentina', 'Activo'
        ),
        (
            8, 'USH', 'Malvinas Argentinas',
            'Ushuaia', 'Argentina', 'Activo'
        ),
        (
            9, 'ROS', 'Islas Malvinas',
            'Rosario', 'Argentina', 'Inactivo'
        );

    -- ==========================================
    -- 4. AERONAVES
    -- ==========================================

    INSERT INTO aeronave (
        id_aeronave,
        matricula,
        modelo,
        estado
    )
    SELECT
        seq,
        CONCAT('LV-D', LPAD(seq, 2, '0')),
        IF(
            MOD(seq, 2) = 0,
            'Airbus A320',
            'Boeing 737-800'
        ),
        IF(seq = 13, 'Inactivo', 'Activo')
    FROM seq_1_to_13;

    -- ==========================================
    -- 5. TIPOS DE EQUIPAJE
    -- ==========================================

    INSERT INTO tipo_equipaje (
        id_tipo_equipaje,
        nombre,
        costo_adicional,
        limite_kg,
        descripcion,
        estado
    )
    VALUES
        (
            1, 'Equipaje de cabina',
            12000, 10,
            'Pieza de cabina hasta 10 kg',
            'Activo'
        ),
        (
            2, 'Equipaje de bodega',
            25000, 23,
            'Pieza de bodega hasta 23 kg',
            'Activo'
        ),
        (
            3, 'Equipaje de bodega grande',
            35000, 32,
            'Pieza de bodega hasta 32 kg',
            'Activo'
        );

    -- ==========================================
    -- 6. PASAJEROS
    -- 580 activos y 20 inactivos.
    -- Todos los datos son ficticios.
    -- ==========================================

    INSERT INTO pasajero (
        id_pasajero,
        nombre,
        apellido,
        documento,
        nacionalidad,
        email,
        telefono,
        estado
    )
    SELECT
        seq,

        ELT(
            1 + MOD(seq - 1, 12),
            'Lucia', 'Mateo', 'Valentina', 'Tomas',
            'Sofia', 'Benjamin', 'Camila', 'Joaquin',
            'Martina', 'Santiago', 'Emma', 'Nicolas'
        ),

        CONCAT(
            ELT(
                1 + MOD(FLOOR((seq - 1) / 12), 10),
                'Gomez', 'Perez', 'Rodriguez',
                'Fernandez', 'Lopez', 'Martinez',
                'Garcia', 'Romero', 'Diaz', 'Torres'
            ),
            ' ',
            LPAD(seq, 3, '0')
        ),

        CAST(40000000 + seq AS CHAR),

        ELT(
            1 + MOD(seq, 5),
            'Argentina', 'Argentina', 'Argentina',
            'Chile', 'Uruguay'
        ),

        CONCAT('pasajero', seq, '@example.test'),

        CONCAT('1100', LPAD(seq, 6, '0')),

        IF(seq > 580, 'Inactivo', 'Activo')

    FROM seq_1_to_600;

    -- ==========================================
    -- 7. PLAN DE VUELOS
    --
    -- IDs 1–28: período anterior, 28 vuelos.
    -- IDs 29–70: período actual, 42 vuelos.
    -- IDs 71–84: próximos siete días.
    -- IDs 85–86: salen en dos y cinco horas.
    --
    -- Ruta 1: AEP → COR
    -- Ruta 2: AEP → MDZ
    -- Ruta 3: AEP → BRC
    -- Ruta 4: EZE → IGR
    -- Ruta 5: COR → AEP
    -- Ruta 6: MDZ → AEP
    -- ==========================================

    INSERT INTO demo_plan (
        id,
        dia,
        ruta,
        objetivo
    )
    SELECT
        seq,

        CASE
            WHEN seq <= 28
                THEN -28 + FLOOR((seq - 1) / 2)
            WHEN seq <= 70
                THEN -14 + FLOOR((seq - 29) / 3)
            WHEN seq <= 84
                THEN 1 + FLOOR((seq - 71) / 2)
            ELSE 0
        END,

        CASE MOD(seq - 1, 10)
            WHEN 0 THEN 1
            WHEN 1 THEN 1
            WHEN 2 THEN 1
            WHEN 3 THEN 2
            WHEN 4 THEN 2
            WHEN 5 THEN 3
            WHEN 6 THEN 3
            WHEN 7 THEN 4
            WHEN 8 THEN 5
            ELSE 6
        END,

        CASE
            WHEN MOD(seq, 13) = 0 THEN 0
            WHEN seq <= 28 THEN 72 + 12 * MOD(seq, 4)
            WHEN seq <= 70 THEN 108 + 12 * MOD(seq, 4)
            WHEN seq <= 84 THEN 48 + 12 * MOD(seq, 4)
            ELSE 24
        END

    FROM seq_1_to_86;

    -- ==========================================
    -- 8. VUELOS
    -- Fechas de salida y llegada en hora local.
    -- ==========================================

    INSERT INTO vuelo (
        id_vuelo,
        id_aeronave,
        id_usuario_operador,
        id_lugar_origen,
        id_lugar_destino,
        numero_vuelo,
        salida,
        llegada,
        estado_vuelo,
        tarifa
    )
    SELECT
        id,

        1 + MOD(id - 1, 12),

        3,

        CASE ruta
            WHEN 4 THEN 5
            WHEN 5 THEN 2
            WHEN 6 THEN 3
            ELSE 1
        END,

        CASE ruta
            WHEN 1 THEN 2
            WHEN 2 THEN 3
            WHEN 3 THEN 4
            WHEN 4 THEN 6
            ELSE 1
        END,

        CONCAT('SK', LPAD(id, 4, '0')),

        salida_demo,

        TIMESTAMPADD(HOUR, 2, salida_demo),

        CASE
            WHEN MOD(id, 13) = 0 THEN 'Cancelado'
            WHEN id <= 70 THEN 'Finalizado'
            WHEN MOD(id, 2) = 0 THEN 'Confirmado'
            ELSE 'Programado'
        END,

        CASE ruta
            WHEN 1 THEN 85000
            WHEN 2 THEN 110000
            WHEN 3 THEN 155000
            WHEN 4 THEN 145000
            WHEN 5 THEN 90000
            ELSE 115000
        END

    FROM (
        SELECT
            p.*,

            CASE
                WHEN id >= 85 THEN
                    TIMESTAMPADD(
                        HOUR,
                        2 + 3 * (id - 85),
                        @ahora_local
                    )
                ELSE
                    TIMESTAMPADD(
                        HOUR,

                        CASE
                            WHEN id <= 28
                                THEN 7 + 6 * MOD(id - 1, 2)
                            WHEN id <= 70
                                THEN 6 + 6 * MOD(id - 29, 3)
                            ELSE
                                8 + 6 * MOD(id - 71, 2)
                        END,

                        TIMESTAMPADD(
                            DAY,
                            dia,
                            CAST(@fecha_base AS DATETIME)
                        )
                    )
            END AS salida_demo

        FROM demo_plan p
    ) datos;

    -- ==========================================
    -- 9. ASIENTOS
    -- 30 filas, letras A–F: 180 por vuelo.
    -- ==========================================

    INSERT INTO asiento (
        id_asiento,
        id_vuelo,
        fila,
        letra,
        estado
    )
    SELECT
        (v.id_vuelo - 1) * 180 + s.seq,
        v.id_vuelo,
        1 + FLOOR((s.seq - 1) / 6),
        CHAR(65 + MOD(s.seq - 1, 6)),
        'Libre'

    FROM vuelo v
    CROSS JOIN seq_1_to_180 s;

    -- ==========================================
    -- 10. RESERVAS
    -- Fechas de operaciones guardadas en UTC.
    -- ==========================================

    INSERT INTO reserva (
        id_reserva,
        id_pasajero,
        id_vuelo,
        id_asiento,
        id_usuario_agente,
        codigo_reserva,
        tarifa_base,
        estado_reserva,
        fecha_reserva,
        fecha_cancelacion,
        fecha_expiracion
    )
    SELECT
        (v.id_vuelo - 1) * 180 + s.seq,

        1 + MOD(
            (v.id_vuelo - 1) * 151 + s.seq - 1,
            580
        ),

        v.id_vuelo,

        (v.id_vuelo - 1) * 180 + s.seq,

        4 + MOD(s.seq, 2),

        CONCAT(
            'D',
            LPAD((v.id_vuelo - 1) * 180 + s.seq, 5, '0')
        ),

        v.tarifa,

        CASE
            WHEN v.estado_vuelo = 'Cancelado'
                THEN 'Cancelada'
            WHEN s.seq <= p.objetivo
                THEN 'Confirmada'
            WHEN s.seq <= p.objetivo + 6
                THEN 'Cancelada'
            WHEN s.seq <= p.objetivo + 12
                THEN 'Expirada'
            ELSE 'PendientePago'
        END,

        CASE
            WHEN s.seq > p.objetivo + 12
                 AND v.estado_vuelo <> 'Cancelado'
                THEN @ahora_utc
            ELSE
                LEAST(
                    TIMESTAMPADD(
                        DAY, -3,
                        TIMESTAMPADD(HOUR, 3, v.salida)
                    ),
                    TIMESTAMPADD(DAY, -1, @ahora_utc)
                )
        END,

        CASE
            WHEN v.estado_vuelo = 'Cancelado'
                 OR s.seq BETWEEN p.objetivo + 1
                              AND p.objetivo + 6
                THEN
                    LEAST(
                        TIMESTAMPADD(
                            DAY, -2,
                            TIMESTAMPADD(HOUR, 3, v.salida)
                        ),
                        TIMESTAMPADD(HOUR, -12, @ahora_utc)
                    )
            ELSE NULL
        END,

        CASE
            WHEN s.seq > p.objetivo + 12
                 AND v.estado_vuelo <> 'Cancelado'
                THEN TIMESTAMPADD(MINUTE, 5, @ahora_utc)

            WHEN s.seq BETWEEN p.objetivo + 7
                           AND p.objetivo + 12
                 AND v.estado_vuelo <> 'Cancelado'
                THEN
                    TIMESTAMPADD(
                        MINUTE, 5,
                        LEAST(
                            TIMESTAMPADD(
                                DAY, -3,
                                TIMESTAMPADD(HOUR, 3, v.salida)
                            ),
                            TIMESTAMPADD(DAY, -1, @ahora_utc)
                        )
                    )
            ELSE NULL
        END

    FROM vuelo v
    JOIN demo_plan p ON p.id = v.id_vuelo
    CROSS JOIN seq_1_to_180 s

    WHERE s.seq <=
        CASE
            WHEN v.estado_vuelo = 'Cancelado' THEN 12
            ELSE
                p.objetivo + 12 +
                IF(v.id_vuelo > 70, 3, 0)
        END;

    UPDATE asiento a
    JOIN reserva r ON r.id_asiento = a.id_asiento

    SET a.estado =
        IF(
            r.estado_reserva IN ('Confirmada', 'PendientePago'),
            'Ocupado',
            'Libre'
        );

    -- ==========================================
    -- 11. PAGOS INICIALES Y DETALLES
    -- Las canceladas se pagaron y luego reembolsaron.
    -- ==========================================

    INSERT INTO pago (
        id_pago,
        id_reserva,
        id_usuario_agente,
        monto,
        metodo_pago,
        fecha_pago,
        estado_pago,
        tipo_pago
    )
    SELECT
        id_reserva,
        id_reserva,
        id_usuario_agente,
        tarifa_base,

        ELT(
            1 + MOD(id_reserva, 3),
            'Efectivo', 'Transferencia', 'Tarjeta'
        ),

        TIMESTAMPADD(MINUTE, 1, fecha_reserva),

        'Registrado',
        'Inicial'

    FROM reserva
    WHERE estado_reserva IN ('Confirmada', 'Cancelada');

    INSERT INTO pago_detalle (
        id_pago_detalle,
        id_pago,
        id_cargo_adicional,
        concepto,
        monto
    )
    SELECT
        id_pago,
        id_pago,
        NULL,
        'Pasaje',
        monto
    FROM pago;

    -- ==========================================
    -- 12. REEMBOLSOS
    -- ==========================================

    INSERT INTO reembolso (
        id_reembolso,
        id_pago,
        id_usuario_agente,
        monto_reembolsado,
        motivo,
        fecha_reembolso,
        estado_reembolso
    )
    SELECT
        r.id_reserva,
        p.id_pago,
        r.id_usuario_agente,
        p.monto,

        IF(
            v.estado_vuelo = 'Cancelado',
            'Cancelación del vuelo',
            'Cancelación del pasajero'
        ),

        r.fecha_cancelacion,
        'Registrado'

    FROM reserva r

    JOIN pago p
        ON p.id_reserva = r.id_reserva

    JOIN vuelo v
        ON v.id_vuelo = r.id_vuelo

    WHERE r.estado_reserva = 'Cancelada';

    -- ==========================================
    -- 13. EQUIPAJES
    -- Activos, pendientes, cancelados y con exceso.
    -- ==========================================

    INSERT INTO equipaje_reserva (
        id_equipaje_reserva,
        id_reserva,
        id_tipo_equipaje,
        peso_kg,
        costo,
        estado,
        fecha_contratacion,
        limite_kg_contratado
    )
    SELECT
        r.id_reserva,
        r.id_reserva,
        t.id_tipo_equipaje,

        CASE
            WHEN r.estado_reserva = 'Cancelada' THEN NULL
            WHEN MOD(r.id_reserva, 10) = 0
                 AND r.id_vuelo > 70 THEN NULL
            WHEN MOD(r.id_reserva, 7) = 0
                THEN t.limite_kg + 2
            ELSE t.limite_kg - 2
        END,

        t.costo_adicional,

        CASE
            WHEN r.estado_reserva = 'Cancelada'
                THEN 'Cancelado'
            WHEN r.id_vuelo > 70
                 AND MOD(r.id_reserva, 8) = 0
                THEN 'PendientePago'
            ELSE 'Activo'
        END,

        TIMESTAMPADD(MINUTE, 2, r.fecha_reserva),

        t.limite_kg

    FROM reserva r

    JOIN tipo_equipaje t
        ON t.id_tipo_equipaje =
            1 + MOD(r.id_reserva, 3)

    WHERE r.estado_reserva IN ('Confirmada', 'Cancelada')
      AND MOD(r.id_reserva, 3) <> 0;

    -- ==========================================
    -- 14. CARGOS DE EQUIPAJE
    -- ==========================================

    INSERT INTO cargo_adicional (
        id_cargo_adicional,
        id_reserva,
        id_equipaje_reserva,
        tipo_cargo,
        descripcion,
        monto,
        estado,
        fecha_creacion
    )
    SELECT
        id_equipaje_reserva,
        id_reserva,
        id_equipaje_reserva,
        'Equipaje',
        'Contratación de equipaje demo',
        costo,

        CASE estado
            WHEN 'Activo' THEN 'Pagado'
            WHEN 'PendientePago' THEN 'Pendiente'
            ELSE 'Cancelado'
        END,

        fecha_contratacion

    FROM equipaje_reserva;

    -- Exceso: 3000 por kg, según TarifasEquipaje.
    INSERT INTO cargo_adicional (
        id_cargo_adicional,
        id_reserva,
        id_equipaje_reserva,
        tipo_cargo,
        descripcion,
        monto,
        estado,
        fecha_creacion
    )
    SELECT
        20000 + id_equipaje_reserva,
        id_reserva,
        id_equipaje_reserva,
        'ExcesoPeso',
        'Exceso demo de 2 kg',

        ROUND(
            (peso_kg - limite_kg_contratado) * 3000,
            2
        ),

        IF(estado = 'Activo', 'Pagado', 'Pendiente'),

        fecha_contratacion

    FROM equipaje_reserva
    WHERE peso_kg > limite_kg_contratado;

    -- ==========================================
    -- 15. PAGOS ADICIONALES
    -- ==========================================

    INSERT INTO pago (
        id_pago,
        id_reserva,
        id_usuario_agente,
        monto,
        metodo_pago,
        fecha_pago,
        estado_pago,
        tipo_pago
    )
    SELECT
        50000 + c.id_cargo_adicional,
        c.id_reserva,
        r.id_usuario_agente,
        c.monto,

        ELT(
            1 + MOD(r.id_reserva, 3),
            'Efectivo', 'Transferencia', 'Tarjeta'
        ),

        TIMESTAMPADD(MINUTE, 1, c.fecha_creacion),

        'Registrado',
        'Adicional'

    FROM cargo_adicional c

    JOIN reserva r
        ON r.id_reserva = c.id_reserva

    WHERE c.estado = 'Pagado';

    INSERT INTO pago_detalle (
        id_pago_detalle,
        id_pago,
        id_cargo_adicional,
        concepto,
        monto
    )
    SELECT
        50000 + id_cargo_adicional,
        50000 + id_cargo_adicional,
        id_cargo_adicional,
        'CargoAdicional',
        monto

    FROM cargo_adicional
    WHERE estado = 'Pagado';

    -- ==========================================
    -- 16. CHECK-INS
    --
    -- Históricos: embarcados y no embarcados.
    -- Vuelos operativos: algunos ya tienen check-in.
    -- No crear check-in si hay equipajes sin resolver.
    -- ==========================================

    INSERT INTO checkin (
        id_checkin,
        id_reserva,
        id_usuario_agente,
        codigo_qr,
        boarding_pass,
        fecha_checkin,
        estado_checkin
    )
    SELECT
        r.id_reserva,
        r.id_reserva,
        6,

        REPLACE(UUID(), '-', ''),

        CONCAT(
            'SKYPLUS | ',
            r.codigo_reserva,
            ' | ',
            p.apellido,
            ', ',
            p.nombre,
            ' | ',
            v.numero_vuelo,
            ' | ',
            a.fila,
            a.letra
        ),

        IF(
            v.id_vuelo <= 70,
            TIMESTAMPADD(
                HOUR, -1,
                TIMESTAMPADD(HOUR, 3, v.salida)
            ),
            @ahora_utc
        ),

        IF(
            v.id_vuelo <= 70
            AND MOD(r.id_reserva, 9) <> 0,
            'Embarcado',
            'Realizado'
        )

    FROM reserva r

    JOIN vuelo v
        ON v.id_vuelo = r.id_vuelo

    JOIN pasajero p
        ON p.id_pasajero = r.id_pasajero

    JOIN asiento a
        ON a.id_asiento = r.id_asiento

    WHERE r.estado_reserva = 'Confirmada'

      AND (
          v.id_vuelo <= 70
          OR v.id_vuelo >= 85
      )

      AND MOD(r.id_reserva, 4) <> 0

      AND NOT EXISTS (
          SELECT 1
          FROM equipaje_reserva e
          WHERE e.id_reserva = r.id_reserva
            AND (
                e.estado = 'PendientePago'
                OR e.peso_kg IS NULL
            )
      )

      AND NOT EXISTS (
          SELECT 1
          FROM cargo_adicional c
          WHERE c.id_reserva = r.id_reserva
            AND c.estado = 'Pendiente'
      );

    COMMIT;

    DROP TEMPORARY TABLE demo_plan;

END$$

DELIMITER ;

-- ==========================================
-- 17. RESULTADOS Y FECHAS PARA LA PRESENTACIÓN
-- ==========================================

SELECT
    'Actual' AS periodo,
    DATE_SUB(@fecha_base, INTERVAL 14 DAY) AS desde,
    DATE_SUB(@fecha_base, INTERVAL 1 DAY) AS hasta

UNION ALL

SELECT
    'Anterior',
    DATE_SUB(@fecha_base, INTERVAL 28 DAY),
    DATE_SUB(@fecha_base, INTERVAL 15 DAY);

SELECT
    estado_vuelo,
    COUNT(*) AS cantidad
FROM vuelo
GROUP BY estado_vuelo;

SELECT
    estado_reserva,
    COUNT(*) AS cantidad
FROM reserva
GROUP BY estado_reserva;

SELECT
    email_corporativo,
    estado_cuenta
FROM usuario;

-- Reservas de los dos vuelos operativos.
SELECT
    r.codigo_reserva,
    v.numero_vuelo,
    v.salida,

    IF(
        c.id_checkin IS NULL,
        'Pendiente de check-in',
        'Check-in realizado'
    ) AS escenario

FROM reserva r

JOIN vuelo v
    ON v.id_vuelo = r.id_vuelo

LEFT JOIN checkin c
    ON c.id_reserva = r.id_reserva

WHERE r.id_vuelo >= 85
  AND r.estado_reserva = 'Confirmada'

ORDER BY
    r.id_vuelo,
    r.id_reserva;