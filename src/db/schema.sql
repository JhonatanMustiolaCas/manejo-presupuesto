-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------
-- Schema mydb
-- -----------------------------------------------------
-- -----------------------------------------------------
-- Schema DB_MANEJO_PRESUPUESTO
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Schema DB_MANEJO_PRESUPUESTO
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `DB_MANEJO_PRESUPUESTO` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci ;
USE `DB_MANEJO_PRESUPUESTO` ;

-- -----------------------------------------------------
-- Table `DB_MANEJO_PRESUPUESTO`.`TiposOperaciones`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `DB_MANEJO_PRESUPUESTO`.`TiposOperaciones` (
  `Id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `Descripcion` VARCHAR(60) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`))
ENGINE = InnoDB
AUTO_INCREMENT = 3
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;

INSERT INTO `DB_MANEJO_PRESUPUESTO`.`TiposOperaciones` (Id, Descripcion)
VALUES (1, "Ingreso"), (2, "Gastos");


-- -----------------------------------------------------
-- Table `DB_MANEJO_PRESUPUESTO`.`Usuarios`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `DB_MANEJO_PRESUPUESTO`.`Usuarios` (
  `Id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `Email` VARCHAR(256) NOT NULL,
  `EmailNormalizado` VARCHAR(256) NOT NULL,
  `PasswordHash` VARCHAR(10000) NOT NULL,
  PRIMARY KEY (`Id`))
ENGINE = InnoDB
AUTO_INCREMENT = 7
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `DB_MANEJO_PRESUPUESTO`.`Categorias`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `DB_MANEJO_PRESUPUESTO`.`Categorias` (
  `Id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `TipoOperacionId` INT UNSIGNED NOT NULL,
  `UsuarioId` INT UNSIGNED NOT NULL,
  `Nombre` VARCHAR(50) NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_Categ_TiposOp` (`TipoOperacionId` ASC) VISIBLE,
  INDEX `FK_Categ_Usua` (`UsuarioId` ASC) VISIBLE,
  CONSTRAINT `FK_Categ_TiposOp`
    FOREIGN KEY (`TipoOperacionId`)
    REFERENCES `DB_MANEJO_PRESUPUESTO`.`TiposOperaciones` (`Id`),
  CONSTRAINT `FK_Categ_Usua`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `DB_MANEJO_PRESUPUESTO`.`Usuarios` (`Id`))
ENGINE = InnoDB
AUTO_INCREMENT = 12
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `DB_MANEJO_PRESUPUESTO`.`TiposCuentas`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `DB_MANEJO_PRESUPUESTO`.`TiposCuentas` (
  `Id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `UsuarioId` INT UNSIGNED NOT NULL,
  `Nombre` VARCHAR(50) NOT NULL,
  `Orden` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_TiposCue_Usua` (`UsuarioId` ASC) VISIBLE,
  CONSTRAINT `FK_TiposCue_Usua`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `DB_MANEJO_PRESUPUESTO`.`Usuarios` (`Id`))
ENGINE = InnoDB
AUTO_INCREMENT = 7
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `DB_MANEJO_PRESUPUESTO`.`Cuentas`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `DB_MANEJO_PRESUPUESTO`.`Cuentas` (
  `Id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `TipoCuentaId` INT UNSIGNED NOT NULL,
  `Nombre` VARCHAR(50) NOT NULL,
  `Balance` DECIMAL(18,2) NOT NULL,
  `Descripcion` VARCHAR(1000) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_Cue_TiposCue` (`TipoCuentaId` ASC) VISIBLE,
  CONSTRAINT `FK_Cue_TiposCue`
    FOREIGN KEY (`TipoCuentaId`)
    REFERENCES `DB_MANEJO_PRESUPUESTO`.`TiposCuentas` (`Id`))
ENGINE = InnoDB
AUTO_INCREMENT = 8
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `DB_MANEJO_PRESUPUESTO`.`Transacciones`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `DB_MANEJO_PRESUPUESTO`.`Transacciones` (
  `Id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `UsuarioId` INT UNSIGNED NOT NULL,
  `FechaTransaccion` DATETIME NOT NULL,
  `Monto` DECIMAL(18,2) NOT NULL,
  `Nota` VARCHAR(1000) NULL DEFAULT NULL,
  `CuentaId` INT UNSIGNED NOT NULL,
  `CategoriaId` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `FK_Trans_Cuen` (`CuentaId` ASC) VISIBLE,
  INDEX `FK_Trans_Usua` (`UsuarioId` ASC) VISIBLE,
  INDEX `FK_Trans_Categ` (`CategoriaId` ASC) VISIBLE,
  CONSTRAINT `FK_Trans_Categ`
    FOREIGN KEY (`CategoriaId`)
    REFERENCES `DB_MANEJO_PRESUPUESTO`.`Categorias` (`Id`),
  CONSTRAINT `FK_Trans_Cuen`
    FOREIGN KEY (`CuentaId`)
    REFERENCES `DB_MANEJO_PRESUPUESTO`.`Cuentas` (`Id`),
  CONSTRAINT `FK_Trans_Usua`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `DB_MANEJO_PRESUPUESTO`.`Usuarios` (`Id`))
ENGINE = InnoDB
AUTO_INCREMENT = 4
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;

USE `DB_MANEJO_PRESUPUESTO` ;

-- -----------------------------------------------------
-- procedure CrearDatosUsuarioNuevo
-- -----------------------------------------------------

DELIMITER $$
USE `DB_MANEJO_PRESUPUESTO`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `CrearDatosUsuarioNuevo`(
	IN _UsuarioId INT
)
BEGIN
	DECLARE _Efectivo VARCHAR(50) DEFAULT 'Efectivo';
	DECLARE _CuentasDeBanco VARCHAR(50) DEFAULT 'Cuentas de Banco';
	DECLARE _Tarjetas VARCHAR(50) DEFAULT 'Tarjetas';

	INSERT INTO TiposCuentas(Nombre, UsuarioId, Orden)
	VALUES 
	(_Efectivo, _UsuarioId, 1),
	(_CuentasDeBanco, _UsuarioId, 2),
	(_Tarjetas, _UsuarioId, 3);

	INSERT INTO Cuentas(Nombre, Balance, TipoCuentaId)
	SELECT Nombre, 0, Id
	FROM TiposCuentas
	WHERE UsuarioId = _UsuarioId;

	INSERT INTO Categorias(Nombre, TipoOperacionId, UsuarioId)
	VALUES
	('Libros', 2, _UsuarioId),
	('Salario', 1, _UsuarioId),
	('Mesada', 1, _UsuarioId),
	('Comida', 2, _UsuarioId),
	('Suscripciones', 2, _UsuarioId);
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure TiposCuentas_Insertar
-- -----------------------------------------------------

DELIMITER $$
USE `DB_MANEJO_PRESUPUESTO`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `TiposCuentas_Insertar`(
	IN _Nombre VARCHAR(60),
	IN _UsuarioId INT UNSIGNED
)
BEGIN
	DECLARE Orden INT;

	SELECT Orden = COALESCE(MAX(Orden), 0) + 1
	FROM DB_MANEJO_PRESUPUESTO.TiposCuentas
	WHERE UsuarioId = _UsuarioId;

	INSERT INTO DB_MANEJO_PRESUPUESTO.TiposCuentas (Nombre, UsuarioId, Orden)
	VALUES (_Nombre, _UsuarioId, Orden);

	SELECT LAST_INSERT_ID();
	
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure Transacciones_Actualizar
-- -----------------------------------------------------

DELIMITER $$
USE `DB_MANEJO_PRESUPUESTO`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `Transacciones_Actualizar`(
	IN _Id INT UNSIGNED,
	IN _FechaTransaccion DATETIME,
	IN _Monto DECIMAL(18, 2),
	IN _MontoAnterior DECIMAL(18, 2),
	IN _CuentaId INT UNSIGNED,
	IN _CuentaAnteriorId INT UNSIGNED,
	IN _CategoriaId INT UNSIGNED,
	IN _Nota VARCHAR(1000)
)
BEGIN
	
	IF _Nota = '' THEN SET _Nota = NULL; END IF;
	
	-- Revertir la transaccion anterior
	
	UPDATE Cuentas
	SET Balance = Balance - _MontoAnterior
	WHERE Id = _CuentaAnteriorId;
	
	
	-- Realizar nueva transaccion

	UPDATE Cuentas
	SET Balance = Balance + _Monto
	WHERE Id = _CuentaId;

	UPDATE Transacciones
	SET Monto = ABS(_Monto),
	FechaTransaccion = _FechaTransaccion,
	CategoriaId = _CategoriaId,
	CuentaId = _CuentaId,
	Nota = _Nota
	WHERE Id = _Id;
	
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure Transacciones_Borrar
-- -----------------------------------------------------

DELIMITER $$
USE `DB_MANEJO_PRESUPUESTO`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `Transacciones_Borrar`(
	IN _Id INT UNSIGNED
)
BEGIN
	DECLARE _Monto DECIMAL(18, 2);
	DECLARE _CuentaId INT;
	DECLARE _TipoOperacionId INT;
	DECLARE _FactorMultiplicativo INT;

	SELECT _Monto = Monto, _CuentaId = CuentaId, _TipoOperacionId = cat.TipoOperacionId
	FROM Transacciones
	INNER JOIN Categorias cat
	ON cat.Id = Transacciones.CategoriaId
	WHERE Transacciones.Id = _Id;

	
	IF _TipoOperacionId = 2 THEN SET _FactorMultiplicativo = -1; END IF;

	SET _Monto = _Monto * _FactorMultiplicativo;

	UPDATE Cuentas
	SET Balance = Balance - _Monto
	WHERE Id = _CuentaId;

	DELETE FROM Transacciones
	WHERE Id = _Id;

END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure Transacciones_Insertar
-- -----------------------------------------------------

DELIMITER $$
USE `DB_MANEJO_PRESUPUESTO`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `Transacciones_Insertar`(
	IN _UsuarioId INT UNSIGNED,
	IN _FechaTransaccion DATE,
	IN _Monto DECIMAL(18, 2),
	IN _CategoriaId INT UNSIGNED,
	IN _CuentaId INT UNSIGNED,
	IN _Nota VARCHAR(1000)
)
BEGIN
	INSERT INTO DB_MANEJO_PRESUPUESTO.Transacciones (
		UsuarioId,
		FechaTransaccion,
		Monto,
		CategoriaId,
		CuentaId,
		Nota
	)
	VALUES (
		_UsuarioId,
		_FechaTransaccion,
		ABS(_Monto),
		_CategoriaId,
		_CuentaId,
		_Nota
	);

	UPDATE Cuentas 
	SET Balance = Balance + _Monto
	WHERE Id = _CuentaId;

	SELECT LAST_INSERT_ID();
END$$

DELIMITER ;

SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
