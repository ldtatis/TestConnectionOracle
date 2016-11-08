using System.Text;
using System.Data.Common;
using System.Configuration;
using System.Data;

using System.Data.Odbc;
using Microsoft.Win32;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Data.SqlClient;
//using System.Data.OracleClient;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
//using Oracle.DataAccess.Client;
//using Oracle.DataAccess.Types;
using System.Data.OracleClient;


namespace MicrocolsaAccesoDatosNet
{
    public class MicrocolsaDatosCorrespondencia
    {
        /// <summary>
        /// Variable que contiene los errores que se generen al momento de crear la base de datos entre otros
        /// </summary>
        public string _txtError;
        /// <summary>
        /// Variable que contiene la informacio de los scripts sql para la creacion actualizacion
        /// </summary>
        public string _txtQuery;
        //Indica cual es el caracter utilziado para los parametros de los
        //comandos de base datos. Para MSSQL se utiliza @ y para MYSQL se
        //utiliza ?
        public string caracterParametro = "@";

        /// <summary>
        /// Tipo de motores de bases de datos
        /// INTERNO | Para bases de datos internas de la aplicacion tipo sql xpress
        /// ORACLEMS | Utiliza el proveedor para System.Dara.OracleClient para conectarse a bases de datos Oracle, sin importar la versión
        /// del cliente de Oracle instalada
        /// ORACLE10G | Solo para instalaciones con el cliente Oracle 10g a 32 bits
        /// </summary>
        public enum tipoMotorDb { MYSQL = 1, MSSQL = 2, ORACLE10G = 3, ODBC = 4, INTERNO = 5, ORACLEMS = 6 };
        /// <summary>
        /// Id de la base de datos
        /// </summary>
        public enum enumIdDb { Radicador = 1, Docuware = 2 };

        /// <summary>
        /// Conexion para bases de datos SQL,MYSQL
        /// </summary>
        public DbConnection conexion = null;

        /// <summary>
        /// Conexión para bases de datos oracle
        /// </summary>
        //public Oracle.DataAccess.Client.OracleConnection oraConexion = null;

        /// <summary>
        /// Cadena de conexión
        /// </summary>
        private string cadenaConexion;
        /// <summary>
        /// Loguin del usuario único para la base de datos
        /// </summary>
        string usuarioDb;
        /// <summary>
        /// Contraseña del usuario de base de datos
        /// </summary>
        string contrasenaDb;
        /// <summary>
        /// Nombre Servidor de base de datos
        /// </summary>
        string servidorDb;
        /// <summary>
        /// Nombre del servicio para acceso a bases de datos Oracle
        /// </summary>
        string serviceName = "";
        /// <summary>
        /// Nombre del servidor de base de datos
        /// </summary>
        string server = "";
        /// <summary>
        /// Nombre de la base de datos
        /// </summary>
        string nombreDb;
        /// <summary>
        /// Indica el motor de base de datos, puede ser MSSQL|MYSQL|ORACLE
        /// </summary>
        tipoMotorDb motorDb;
        /// <summary>
        /// Proveedor para la base de datos
        /// </summary>
        string proveedor;
        /// <summary>
        /// Origen de datos para la base de datos MySql
        /// </summary>
        string dsnDb;
        /// <summary>
        /// Objeto utilizado para desencriptar la contraseña
        /// </summary>
        /// 
        //Encripcion encripcion = new Encripcion();
        /// <summary>
        /// Puerto para la base de datos de MySQL
        /// </summary>
        string mySqlPort;

        /// <summary>
        /// Ruta local de la base de datos interna Ejemplo C:\Microcolsa\Microcolsa Scan\Desarrollo\Microcolsa Scan\Microcolsa Scan\WinScan\dBaseMicrocolsaScan.mdf
        /// </summary>
        public string localDataBase;
        /// <summary>
        /// Clave de encripción utilizada para encriptar los textos
        /// </summary>
        public bool existeTabla;
        private string claveEncripcion = "/*esm90tyi812323lopoasdfopasd04";
        //private static DbProviderFactory factory = null;
        protected DbProviderFactory factory;
        /// <summary>
        /// Comando para bases de datos MySQL y SQL
        /// </summary>
        public DbCommand comando = null;
        /// <summary>
        /// Comando de Oracle
        /// </summary>
        //public Oracle.DataAccess.Client.OracleCommand oraComando = null;
        private DbTransaction transaccion = null;
        //private Oracle.DataAccess.Client.OracleTransaction oraTransaccion = null;
        private DbDataAdapter adaptador = null;

        //Comando utilizado para ejecutar los procedimientos almacenados del tipo SQLServer
        public System.Data.SqlClient.SqlCommand sqlComando = null;

        /// <summary>
        /// Corrige el caracter de los parametros en las sentencias
        /// SQL. Para MSSQL se usa @ y para MYSQL se usa ?.
        /// </summary>
        /// <param name="strSql">Sentencia SQL a la que se le corregira el 
        /// parametro</param>
        /// <returns></returns>
        public string CorregirCaracterParametroSql(string strSql)
        {
            return (strSql.Replace("@", this.caracterParametro));
        }

        /// <summary>
        /// Devuelve el nombre de la funcion IsNull o IfNull de acuerdo al motor de Db
        /// MSSQL usa IsNull y MySQL usa IfNull
        /// </summary>
        /// <returns></returns>
        public string IsNull()
        {
            string isNull = "IsNull";
            switch (motorDb)
            {
                case tipoMotorDb.MYSQL:
                    isNull = "IfNull";
                    break;
            }
            return (isNull);
        }


        /// <summary>
        /// Lee la información del registro para conectarse a la base de datos del radicador
        /// </summary>
        /// 
        /*
        public void ConfigurarRadicador(EntornoServidor entornoServidor)
        {
            try
            {


                this.servidorDb = entornoServidor.ServidorDBRadicador;
                this.nombreDb = entornoServidor.DBRadicador;
                this.dsnDb = entornoServidor.DsnDbRadicador;
                this.usuarioDb = entornoServidor.UsuarioDBRadicador;
                this.contrasenaDb = entornoServidor.ContrasenaDBRadicador;
                this.motorDb = entornoServidor.MotorDBRadicador;
                this.mySqlPort = entornoServidor.PuertoDbRadicador;
                switch (this.motorDb)
                {
                    case BaseDatosRadicadorWindows.tipoMotorDb.MYSQL:
                        this.caracterParametro = "?";
                        //proveedor = "System.Data.Odbc";
                        this.proveedor = "MySql.Data.MySqlClient";                        
                        //cadenaConexion = "Dsn=" + dsnDb + ";database=" + nombreDb + ";option=0;port=0;server=" + servidorDb + ";uid=" + usuarioDb + ";pwd=" + contrasenaDb;
                        //this.cadenaConexion = "server=" + servidorDb + ";user id=" + usuarioDb + ";Password=" + contrasenaDb + ";persist security info=False;database=" + nombreDb + ";MultipleActiveResultSets=true;";

                        if (mySqlPort!=null)
                        {
                            this.cadenaConexion = "server=" + servidorDb + ";user id=" + usuarioDb + ";Password=" + contrasenaDb + ";persist security info=False;database=" + nombreDb + ";port=" + this.mySqlPort;
                        }
                        else
                        {
                            this.cadenaConexion = "server=" + servidorDb + ";user id=" + usuarioDb + ";Password=" + contrasenaDb + ";persist security info=False;database=" + nombreDb + ";";
                        }
                        break;
                    case BaseDatosRadicadorWindows.tipoMotorDb.MSSQL:
                        this.caracterParametro = "@";
                        this.proveedor = "System.Data.SqlClient";
                        //MultipleActiveResultSets
                        //this.cadenaConexion = "Data Source=" + servidorDb + ";Initial Catalog=" + nombreDb + ";Persist Security Info=True;User ID=" + usuarioDb + ";Password=" + contrasenaDb + ";";
                        this.cadenaConexion = "Data Source=" + servidorDb + ";Initial Catalog=" + nombreDb + ";Persist Security Info=True;User ID=" + usuarioDb + ";Password=" + contrasenaDb + ";Connect TimeOut=120";
                        //this.cadenaConexion = "Data Source=" + servidorDb + ";Initial Catalog=" + nombreDb + ";Persist Security Info=True;User ID=" + usuarioDb + ";Password=" + contrasenaDb + ";MultipleActiveResultSets=true";
                        break;
                    case BaseDatosRadicadorWindows.tipoMotorDb.ORACLE:
                        this.caracterParametro = "@";
                        this.proveedor = "System.Data.OracleClient";
                        this.cadenaConexion = "";

                        //Data Source=ServerName:PortNumber/ServiceName;User ID=UserLogin;Password=pwd

                        this.cadenaConexion = ";User ID=" + usuarioDb + ";Password=" + contrasenaDb + ";Data Source=";

                        if (servidorDb.Length > 0)
                        {
                            this.cadenaConexion += servidorDb;
                        }

                        if (mySqlPort.Length > 0)
                        {
                            this.cadenaConexion += ":" + this.mySqlPort;
                        }

                        if (serviceName.Length > 0)
                        {
                            this.cadenaConexion += "/" + this.serviceName;
                        }
                        break;
                    case tipoMotorDb.ODBC:
                        this.caracterParametro = "@";
                        this.proveedor = "System.Data.Odbc";
                        this.cadenaConexion = "Dsn=" + dsnDb + ";uid=" + usuarioDb + ";Password=" + contrasenaDb + ";MultipleActiveResultSets=true;";
                        break;
                }

                //BaseDatosRadicadorWindows.factory = DbProviderFactories.GetFactory(this.proveedor);
                this.factory = DbProviderFactories.GetFactory(this.proveedor);
                this.conexion = factory.CreateConnection();
                this.conexion.ConnectionString = cadenaConexion;
            }
            catch (Exception ex)
            {
                string mensajeError;
                mensajeError = "Descripción: " + ex.Message +
                "Origen: " + "BaseDatos.ConfigurarRadicador()" +
                "Descripción adicional: ";

                throw new Exception(mensajeError);

            }
        }
         */
        /// <summary>
        /// Configura una base de datos diferente a la de radicacion
        /// <param name="serviceName">Servicio de red de Oracle</param>
        /// </summary>
        public void ConfigurarDb(string usr, string pwd, string db, string origenDatos, string server, tipoMotorDb motor, string mySqlPort, string serviceName, string localDbFile)
        {
            try
            {

                this.servidorDb = server;
                this.nombreDb = db;
                this.dsnDb = origenDatos;
                this.usuarioDb = usr;
                this.contrasenaDb = pwd;
                this.motorDb = motor;
                this.mySqlPort = mySqlPort;
                this.serviceName = serviceName;
                this.localDataBase = localDbFile;

                //this.localDataBase = rutadbaseLocal;

                /*

                if (usuarioDb.Length == 0) {
                    throw new Exception("El parámetro usuario no puede estar vacio");
                }
                */
                /*
                if (contrasenaDb.Length == 0) {
                    throw new Exception("El parámetro contraseña no puede estar vacio");
                }
                */



                switch (this.motorDb)
                {
                    case tipoMotorDb.MYSQL:
                        this.caracterParametro = "?";
                        //proveedor = "System.Data.Odbc";
                        this.proveedor = "MySql.Data.MySqlClient";
                        //cadenaConexion = "Dsn=" + dsnDb + ";database=" + nombreDb + ";option=0;port=0;server=" + servidorDb + ";uid=" + usuarioDb + ";pwd=" + contrasenaDb;

                        if (mySqlPort != null)
                        {

                            this.cadenaConexion = "server=" + servidorDb + ";user id=" + usuarioDb + ";Password=" + contrasenaDb + ";persist security info=False;database=" + nombreDb + ";Connect Timeout=3600;port=" + mySqlPort;
                        }
                        else
                        {
                            this.cadenaConexion = "server=" + servidorDb + ";user id=" + usuarioDb + ";Password=" + contrasenaDb + ";persist security info=False;database=" + nombreDb + ";Connect Timeout=3600";
                        }

                        break;
                    case tipoMotorDb.INTERNO:
                        this.proveedor = "System.Data.SqlClient";
                        //this.cadenaConexion = @"Data Source=.\SQLEXPRESS;AttachDbFilename=" + localDataBase + ";Integrated Security=True;User Instance=True";
                        this.cadenaConexion = @"Data Source=.\SQLEXPRESS;AttachDbFilename=" + this.localDataBase + ";Integrated Security=True;User Instance=True";
                        break;
                    case tipoMotorDb.MSSQL:
                        this.caracterParametro = "@";
                        this.proveedor = "System.Data.SqlClient";
                        if (this.mySqlPort != null)
                        {
                            if (this.mySqlPort.Length > 0)
                            {
                                //this.cadenaConexion = "Data Source=" + servidorDb + ";Initial Catalog=" + nombreDb + ";Persist Security Info=True;User ID=" + usuarioDb + ";Password=" + contrasenaDb + ";Connect Timeout=3600;port=" + this.mySqlPort + ";";
                                this.cadenaConexion = "Data Source=" + servidorDb + "," + this.mySqlPort + ";Initial Catalog=" + nombreDb + ";Persist Security Info=True;User ID=" + usuarioDb + ";Password=" + contrasenaDb + ";Connect Timeout=3600;";
                            }
                            else
                            {
                                this.cadenaConexion = "Data Source=" + servidorDb + ";Initial Catalog=" + nombreDb + ";Persist Security Info=True;User ID=" + usuarioDb + ";Password=" + contrasenaDb + ";Connect Timeout=3600;";
                            }
                        }
                        else
                        {
                            this.cadenaConexion = "Data Source=" + servidorDb + ";Initial Catalog=" + nombreDb + ";Persist Security Info=True;User ID=" + usuarioDb + ";Password=" + contrasenaDb + ";Connect Timeout=3600;";
                        }

                        break;
                    case tipoMotorDb.ORACLEMS:
                        this.caracterParametro = "@";
                        this.proveedor = "System.Data.OracleClient";
                        //this.cadenaConexion = "User Id=" + usuarioDb + ";Password=" + contrasenaDb + ";Data Source=" + serviceName + ";Connection Timeout=240;Persist Security Info=true";

                        /*
                        if (!serviceName.ToUpper().Contains("SID")) {
                            throw new Exception("La cadena de conexión: " + serviceName + ", no contiene la defincicón del SID de Oracle.");
                        }
                        */

                        this.cadenaConexion = "User Id=" + usuarioDb + ";Password=" + contrasenaDb + ";Data Source=" + serviceName + ";Persist Security Info=true";

                        break;
                    case tipoMotorDb.ORACLE10G:
                        /*
                        this.caracterParametro = "@";
                        

                        this.cadenaConexion = "User Id=" + usuarioDb + ";Password=" + contrasenaDb + ";Data Source=" + serviceName + ";Connection Timeout=240;Persist Security Info=true";

                        

                        this.oraConexion = new Oracle.DataAccess.Client.OracleConnection(this.cadenaConexion);
                        */
                        break;
                    case tipoMotorDb.ODBC:
                        this.caracterParametro = "@";
                        this.proveedor = "System.Data.Odbc";
                        this.cadenaConexion = "Dsn=" + dsnDb + ";uid=" + usuarioDb + ";Password=" + contrasenaDb;
                        break;
                }

                //BaseDatosRadicadorWindows.factory = DbProviderFactories.GetFactory(this.proveedor);
                if (this.proveedor != null)
                {
                    if (this.proveedor.Length > 0)
                    {
                        this.factory = DbProviderFactories.GetFactory(this.proveedor);
                        this.conexion = factory.CreateConnection();
                        this.conexion.ConnectionString = cadenaConexion;
                    }
                }

            }
            catch (Exception ex)
            {
                string mensajeError;
                mensajeError = "Descripción: " + ex.Message +
                "Origen: " + "BaseDatos.ConfigurarDb()" +
                "Descripción adicional: ";

                throw new Exception(mensajeError);

            }
        }


        /// <summary>
        /// Configura una base de datos diferente a la de radicacion
        /// <param name="serviceName">Servicio de red de Oracle</param>
        /// </summary>
        /*
        public void ConfigurarDb(ParametrosConexionDB parametrosConexion)
        {
            try
            {



                this.servidorDb = parametrosConexion.Servidor;
                this.nombreDb = parametrosConexion.BaseDatos;
                this.dsnDb = parametrosConexion.Dsn;
                this.usuarioDb = parametrosConexion.Usuario;
                this.contrasenaDb = parametrosConexion.Contrasena;
                this.motorDb = parametrosConexion.MotorDb;
                this.mySqlPort = parametrosConexion.Puerto;
                this.serviceName = parametrosConexion.ServiceName;
                

                if (usuarioDb.Length == 0)
                {
                    throw new Exception("El parámetro usuario no puede estar vacio");
                }

                if (contrasenaDb.Length == 0)
                {
                    throw new Exception("El parámetro contraseña no puede estar vacio");
                }




                switch (this.motorDb)
                {
                    case BaseDatosRadicadorWindows.tipoMotorDb.MYSQL:
                        this.caracterParametro = "?";
                        //proveedor = "System.Data.Odbc";
                        this.proveedor = "MySql.Data.MySqlClient";
                        //cadenaConexion = "Dsn=" + dsnDb + ";database=" + nombreDb + ";option=0;port=0;server=" + servidorDb + ";uid=" + usuarioDb + ";pwd=" + contrasenaDb;

                        if (mySqlPort != null)
                        {

                            this.cadenaConexion = "server=" + servidorDb + ";user id=" + usuarioDb + ";Password=" + contrasenaDb + ";persist security info=False;database=" + nombreDb + ";port=" + mySqlPort;
                        }
                        else
                        {
                            this.cadenaConexion = "server=" + servidorDb + ";user id=" + usuarioDb + ";Password=" + contrasenaDb + ";persist security info=False;database=" + nombreDb;
                        }

                        break;
                    case BaseDatosRadicadorWindows.tipoMotorDb.MSSQL:
                        this.caracterParametro = "@";
                        this.proveedor = "System.Data.SqlClient";
                        this.cadenaConexion = "Data Source=" + servidorDb + ";Initial Catalog=" + nombreDb + ";Persist Security Info=True;User ID=" + usuarioDb + ";Password=" + contrasenaDb + ";Connect Timeout=120;";
                        break;
                    case BaseDatosRadicadorWindows.tipoMotorDb.ORACLE:
                        this.caracterParametro = "@";
                        this.proveedor = "System.Data.OracleClient";
                        //this.cadenaConexion = "Data Source=" + servidorDb + ";Persist Security Info=False;User ID=" + usuarioDb + ";Password=" + contrasenaDb + ";Unicode=True";

                        //this.cadenaConexion = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=" + server + ")(PORT=" + mySqlPort + ")))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=" + serviceName + ")));User Id=" + usuarioDb + ";Password=" + contrasenaDb + ";";

                        //Data Source=ServerName:PortNumber/ServiceName;User ID=UserLogin;Password=pwd

                        this.cadenaConexion = "User ID=" + usuarioDb + ";Password=" + contrasenaDb + ";Data Source=";

                        if (servidorDb.Length > 0)
                        {
                            this.cadenaConexion += servidorDb;
                        }
                        if (mySqlPort.Length > 0)
                        {
                            this.cadenaConexion += ":" + this.mySqlPort;
                        }

                        if (serviceName.Length > 0)
                        {
                            this.cadenaConexion += "/" + this.serviceName;
                        }

                        break;
                    case tipoMotorDb.ODBC:
                        this.caracterParametro = "@";
                        this.proveedor = "System.Data.Odbc";
                        this.cadenaConexion = "Dsn=" + dsnDb + ";uid=" + usuarioDb + ";Password=" + contrasenaDb;
                        break;
                }

                //BaseDatosRadicadorWindows.factory = DbProviderFactories.GetFactory(this.proveedor);
                this.factory = DbProviderFactories.GetFactory(this.proveedor);
                this.conexion = factory.CreateConnection();
                this.conexion.ConnectionString = cadenaConexion;

            }
            catch (Exception ex)
            {
                string mensajeError;
                mensajeError = "Descripción: " + ex.Message +
                "Origen: " + "BaseDatos.ConfigurarDb()" +
                "Descripción adicional: ";

                throw new Exception(mensajeError);

            }
        }
        */


        /// <summary>
        /// Lee la información del registro para conectarse a la base de datos de Docuware
        /// </summary> 
        /// 
        /*
        public  void ConfigurarDocuware(EntornoServidor entornoServidor)
        {
            try
            {

                servidorDb = entornoServidor.ServidorDBDocuware;
                nombreDb = entornoServidor.DBDocuware;
                dsnDb = entornoServidor.DsnDbDocuware;
                usuarioDb = entornoServidor.UsuarioDBDocuware;
                contrasenaDb = entornoServidor.ContrasenaDBDocuware;
                motorDb = entornoServidor.MotorDBDocuware;
                mySqlPort = entornoServidor.PuertoDbDocuware;

                switch (motorDb)
                {
                    case BaseDatosRadicadorWindows.tipoMotorDb.MYSQL:
                        proveedor = "System.Data.Odbc";
                        cadenaConexion = "Dsn=" + dsnDb + ";database=" + nombreDb + ";option=0;port=0;server=" + servidorDb + ";uid=" + usuarioDb + ";pwd=" + contrasenaDb;
                        break;
                    case BaseDatosRadicadorWindows.tipoMotorDb.MSSQL:
                        proveedor = "System.Data.SqlClient";
                        cadenaConexion = "Data Source=" + servidorDb + ";Initial Catalog=" + nombreDb + ";Persist Security Info=True;User ID=" + usuarioDb + ";Password=" + contrasenaDb + ";";
                        break;
                    case BaseDatosRadicadorWindows.tipoMotorDb.ORACLE:
                        proveedor = "System.Data.OracleClient";
                        cadenaConexion = "";
                        //Data Source=ServerName:PortNumber/ServiceName;User ID=UserLogin;Password=pwd

                        this.cadenaConexion = ";User ID=" + usuarioDb + ";Password=" + contrasenaDb + ";Data Source=";

                        if (servidorDb.Length > 0)
                        {
                            this.cadenaConexion += servidorDb;
                        }

                        if (mySqlPort.Length > 0)
                        {
                            this.cadenaConexion += ":" + this.mySqlPort;
                        }

                        if (serviceName.Length > 0)
                        {
                            this.cadenaConexion += "/" + this.serviceName;
                        }
                        break;
                }

                //BaseDatosRadicadorWindows.factory = DbProviderFactories.GetFactory(proveedor);
                this.factory = DbProviderFactories.GetFactory(proveedor);

                conexion = factory.CreateConnection();
                conexion.ConnectionString = cadenaConexion;

            }
            catch (Exception ex)
            {
                string mensajeError;
                mensajeError = "Descripción: " + ex.Message +
                "Origen: " + "BaseDatos.ConfigurarDocuware()" +
                "Descripción adicional: ";

                throw new Exception(mensajeError);

            }
        }
        */
        /// <summary>
        /// Constructor de la clase
        /// </summary>
        public MicrocolsaDatosCorrespondencia()
        {

        }
        /// <summary>
        /// Se concecta con la base de datos.
        /// </summary>
        /// <exception cref="BaseDatosException">Si existe un error al conectarse.</exception>
        public void Conectar()
        {

            try
            {

                switch (this.motorDb)
                {
                    case tipoMotorDb.ORACLE10G:

                        /*
                        if (oraConexion.State == System.Data.ConnectionState.Closed)
                        {
                            oraConexion.Open();
                        }
                        */

                        break;
                    case tipoMotorDb.ORACLEMS:
                        {
                            using (OracleConnection cnn = new OracleConnection(cadenaConexion))
                            {
                                cnn.Open();
                                cnn.Close();
                                cnn.Dispose();
                            }
                        }break;
                    default:

                        if (conexion.State == System.Data.ConnectionState.Closed)
                        {
                            conexion.Open();
                        }

                        break;


                }


            }
            catch (Exception ex)
            {
                string mensajeError;
                mensajeError = "Descripción: " + ex.Message + "\n" +
                "Origen: " + "BaseDatosRadicadorWindows.Conectar()\n" +
                "Descripción adicional: Error al conectarse a la base de datos.\n";
                if (ex.InnerException != null)
                {
                    mensajeError += ex.InnerException.Message;
                }

                throw new Exception(mensajeError);
            }
        }
        /// <summary>
        /// Permite desconectarse de la base de datos.
        /// </summary>
        public void Desconectar()
        {
            //if (conexion.State.Equals(ConnectionState.Open))
            //{
            //if (conexion.State != ConnectionState.Closed)
            //{
            switch (this.motorDb)
            {
                case tipoMotorDb.ORACLE10G:
                    /*
                    if (this.oraConexion.State == ConnectionState.Open)
                    {
                        oraConexion.Close();
                    }
                     */
                    break;
                default:
                    if (this.conexion != null)
                    {
                        if (this.conexion.State == ConnectionState.Open)
                        {
                            conexion.Close();
                        }
                    }
                    break;
            }

            //}
            //}
        }

        /// <summary>
        /// Crea un comando en base a una sentencia SQL.
        /// Ejemplo:
        /// <code>SELECT * FROM Tabla WHERE campo1=@campo1, campo2=@campo2</code>
        /// Guarda el comando para el seteo de parámetros y la posterior ejecución.
        /// </summary>
        /// <param name="sentenciaSQL">La sentencia SQL con el formato: SENTENCIA [param = @param,]</param>
        public void CrearComando(string sentenciaSQL)
        {

            #region Crear el objeto command
            switch (this.motorDb)
            {
                case tipoMotorDb.ORACLE10G:
                    /*
                    if (this.oraConexion.State == ConnectionState.Closed) 
                    {
                        this.Conectar();
                    }

                    this.oraComando = this.oraConexion.CreateCommand();
                    this.oraComando.CommandType = CommandType.Text;
                    this.oraComando.CommandTimeout = 3600;
                     */
                    break;
                default:

                    if (this.conexion.State == ConnectionState.Closed)
                    {
                        this.Conectar();
                    }

                    this.comando = factory.CreateCommand();
                    this.comando.Connection = this.conexion;

                    this.comando.CommandType = CommandType.Text;
                    this.comando.CommandTimeout = 3600;
                    break;
            }
            #endregion Crear el objeto command

            #region Preparar la sentencia SQL
            switch (this.motorDb)
            {
                case tipoMotorDb.MYSQL:
                    this.comando.CommandText = MicrocolsaDatosCorrespondencia.GenerarSentenciaMySqlStatic(sentenciaSQL);
                    this.comando.CommandText = this.CorregirCaracterParametroSql(this.comando.CommandText);
                    break;
                case tipoMotorDb.ORACLE10G:
                    /*
                    this.oraComando.CommandText = this.GenerarSentenciaOracleStatic(sentenciaSQL);
                    
                    */
                    break;
                default:
                    this.comando.CommandText = sentenciaSQL;
                    break;
            }

            #endregion Preparar la sentencia SQL

            #region Trabajar con transaccion
            switch (this.motorDb)
            {
                case tipoMotorDb.ORACLE10G:
                    /*
                    this.oraComando.CommandTimeout = 120;
                    if (this.transaccion != null) {
                        
                        //Falta agregar la parte de la transaccion
                    }
                    */
                    break;
                default:
                    this.comando.CommandTimeout = 120;
                    if (this.transaccion != null)
                    {
                        this.comando.Transaction = this.transaccion;
                    }
                    break;
            }
            #endregion Trabajar con transaccion

        }

        /// <summary>
        /// Crea un comando en base a una sentencia SQL.
        /// Ejemplo:
        /// <code>SELECT * FROM Tabla WHERE campo1=@campo1, campo2=@campo2</code>
        /// Guarda el comando para el seteo de parámetros y la posterior ejecución.
        /// </summary>
        /// <param name="sentenciaSQL">La sentencia SQL con el formato: SENTENCIA [param = @param,]</param>
        public void CrearComando(string sentenciaSQL, System.Data.CommandType tipoComando)
        {


            this.Conectar();

            #region Crear la instancia del objeto Command

            switch (this.motorDb)
            {
                case tipoMotorDb.ORACLE10G:

                    /*
                    this.oraComando = this.oraConexion.CreateCommand();
                    this.oraComando.CommandType = tipoComando;
                    this.oraComando.CommandTimeout = 3600;
                    */
                    break;

                default:

                    this.comando = factory.CreateCommand();
                    this.comando.Connection = this.conexion;
                    this.comando.CommandType = tipoComando;
                    this.comando.CommandTimeout = 3600;

                    break;
            }

            #endregion Crear la instancia del objeto Command

            #region Asignar la sentencia SQL

            switch (this.motorDb)
            {
                case tipoMotorDb.MYSQL:
                    this.comando.CommandText = MicrocolsaDatosCorrespondencia.GenerarSentenciaMySqlStatic(sentenciaSQL);
                    this.comando.CommandText = this.CorregirCaracterParametroSql(this.comando.CommandText);
                    break;
                case tipoMotorDb.ORACLE10G:
                    /*
                    this.oraComando.CommandText = this.GenerarSentenciaOracleStatic(sentenciaSQL);
                    */
                    break;
                default:
                    this.comando.CommandText = sentenciaSQL;
                    break;
            }

            #endregion Asignar la sentencia SQL

            #region Utilizar la transaccion

            switch (this.motorDb)
            {
                case tipoMotorDb.ORACLE10G:

                    /*
                    this.oraComando.CommandTimeout = 240;

                    if(this.oraTransaccion != null){
                        throw new Exception("No se ha implementado el uso de transacciones con motor ORACLE");
                    }
                    */
                    break;

                default:

                    this.comando.CommandTimeout = 120;
                    if (this.transaccion != null)
                    {
                        this.comando.Transaction = this.transaccion;
                    }

                    break;
            }

            #endregion Utilizar la transaccion
        }

        /// <summary>
        /// Asigna los parametros a un comando SQL
        /// </summary>
        /// <param name="nombre"></param>
        /// <param name="separador"></param>
        /// <param name="valor"></param>
        private void AsignarParametro(string nombre, string separador, string valor)
        {
            try
            {

                switch (this.motorDb)
                {
                    case tipoMotorDb.ORACLE10G:
                        {
                            /*
                            int indice = this.oraComando.CommandText.IndexOf(nombre);
                            string prefijo = this.oraComando.CommandText.Substring(0, indice);
                            string sufijo = this.oraComando.CommandText.Substring(indice + nombre.Length);
                            this.oraComando.CommandText = prefijo + separador + valor + separador + sufijo;
                            */
                        }

                        break;
                    default:
                        {
                            int indice = this.comando.CommandText.IndexOf(nombre);
                            string prefijo = this.comando.CommandText.Substring(0, indice);
                            string sufijo = this.comando.CommandText.Substring(indice + nombre.Length);
                            this.comando.CommandText = prefijo + separador + valor + separador + sufijo;
                        }

                        break;
                }



            }
            catch (Exception ex)
            {

                string mensaje = "Descripción: No se pudo asignar el paramentro" +
                    "\nDescripción: " + ex.Message +
                    "\nOrigen: MicrocolsaDatosCorrespondencia.AsignarParametro()";
                throw new Exception(mensaje);

            }
        }

        /// <summary>
        /// Asigna un parámetro de tipo cadena al comando creado.
        /// </summary>
        /// <param name="nombre">El nombre del parámetro.</param>
        /// <param name="valor">El valor del parámetro.</param>
        public void AsignarParametroCadena(string nombre, string valor)
        {
            try
            {
                AsignarParametro(nombre, "'", valor);
            }
            catch (Exception ex)
            {

                string mensaje = "Descripción: No se pudo asignar el paramentro" +
                    "\nDescripción: " + ex.Message +
                    "\nOrigen: MicrocolsaDatosCorrespondencia.AsignarParametroCadena()";

                throw new Exception(mensaje);

            }
        }

        /// <summary>
        /// Asigna un parámetro de tipo cadena, que no va entre comillas, al comando creado.
        /// esto se usa para setencias DDL
        /// </summary>
        /// <param name="nombre">El nombre del parámetro.</param>
        /// <param name="valor">El valor del parámetro.</param>
        public void AsignarParametroNoCadena(string nombre, string valor)
        {
            try
            {
                AsignarParametro(nombre, "", valor);
            }
            catch (Exception ex)
            {

                string mensaje = "Descripción: No se pudo asignar el paramentro" +
                    "\nDescripción: " + ex.Message +
                    "\nOrigen: MicrocolsaDatosCorrespondencia.AsignarParametroNoCadena()";

                throw new Exception(mensaje);

            }
        }

        /// <summary>
        /// Asigna un parámetro de tipo entero al comando creado.
        /// </summary>
        /// <param name="nombre">El nombre del parámetro.</param>
        /// <param name="valor">El valor del parámetro.</param>
        public void AsignarParametroEntero(string nombre, int valor)
        {
            try
            {
                AsignarParametro(nombre, "", valor.ToString());
            }
            catch (Exception ex)
            {

                string mensaje = "Descripción: No se pudo asignar el paramentro" +
                    "\nDescripción: " + ex.Message +
                    "\nOrigen: MicrocolsaDatosCorrespondencia.AsignarParametroEntero()";

                throw new Exception(mensaje);

            }
        }

        /// <summary>
        /// Asigna un parámetro de tipo long al comando creado.
        /// </summary>
        /// <param name="nombre">El nombre del parámetro.</param>
        /// <param name="valor">El valor del parámetro.</param>
        public void AsignarParametroLong(string nombre, long valor)
        {
            try
            {
                AsignarParametro(nombre, "", valor.ToString());
            }
            catch (Exception ex)
            {

                string mensaje = "Descripción: No se pudo asignar el paramentro" +
                    "\nDescripción: " + ex.Message +
                    "\nOrigen: MicrocolsaDatosCorrespondencia.AsignarParametroLong()";

                throw new Exception(mensaje);

            }
        }

        /// <summary>
        /// Ejecuta el comando creado y retorna el resultado de la consulta.
        /// </summary>
        /// <returns>El resultado de la consulta.</returns>
        /// <exception cref="BaseDatosException">Si ocurre un error al ejecutar el comando.</exception>
        public object EjecutarConsulta()
        {
            DbDataReader dbdr = null;
            //Oracle.DataAccess.Client.OracleDataReader oraDbdr = null;

            string sql = "";
            try
            {



                switch (this.motorDb)
                {
                    case tipoMotorDb.MYSQL:

                        sql = MicrocolsaDatosCorrespondencia.GenerarSentenciaMySqlStatic(this.comando.CommandText);

                        this.comando.CommandText = sql;
                        dbdr = this.comando.ExecuteReader();

                        break;
                    case tipoMotorDb.ORACLE10G:
                        /*
                        sql = this.GenerarSentenciaOracleStatic(this.oraComando.CommandText);

                        this.oraComando.CommandText = sql;
                        oraDbdr = this.oraComando.ExecuteReader();                        
                        */
                        break;
                    default:

                        dbdr = this.comando.ExecuteReader();

                        break;

                }



            }
            catch (Exception ex)
            {
                string mensaje = "Descripción: No se pudo ejecutar la sentencia SQL" +
                    "\nOrigen: BaseDatosRadicadorWindows.EjecutarConsulta()" +
                    "\nSQL: " + sql +
                    "\nDescripción: " + ex.Message;
                throw new Exception(mensaje);

            }

            /*
            switch (this.motorDb) {
                case tipoMotorDb.ORACLE10G:                    
                    return oraDbdr;
                    break;
                default:
                    return dbdr;
                    break;
            }
             */

            return dbdr;


        }

        /// <summary>
        /// Devuelve el resultado de una consulta en un DataSet
        /// </summary>
        /// <param name="nombreTabla">Nombre de la tabla que se crea en el DataSet</param>
        /// <returns></returns>
        public System.Data.DataSet EjecutarConsultaDS(string nombreTabla)
        {
            DataSet ds = new DataSet();

            try
            {
                switch (this.motorDb)
                {
                    case tipoMotorDb.ORACLE10G:
                        /*
                        {
                            
                            Oracle.DataAccess.Client.OracleDataAdapter adaptador = new Oracle.DataAccess.Client.OracleDataAdapter(this.oraComando);                            

                            adaptador.Fill(ds, nombreTabla);
                        }
                        */
                        break;

                    default:
                        {
                            DbDataAdapter adaptador = factory.CreateDataAdapter();

                            adaptador.SelectCommand = this.comando;

                            adaptador.Fill(ds, nombreTabla);

                        }

                        break;
                }



                return ds;
            }
            catch (Exception ex)
            {
                string mensaje = "Descripción: No se pudo ejecutar la sentencia SQL" +
                    "\nOrigen: MicrocolsaDatosCorrespondencia.EjecutarConsultaDS()";

                if (this.comando != null)
                {
                    mensaje += "\nSQL: " + this.comando.CommandText;
                }


                mensaje += "\nDescripción: " + ex.Message;
                throw new Exception(mensaje);

            }
        }

        /// <summary>
        /// Ejecuta el comando creado.
        /// </summary>
        public void EjecutarComando()
        {
            try
            {


                switch (this.motorDb)
                {
                    case tipoMotorDb.MYSQL:
                        {
                            if (this.conexion.State == ConnectionState.Closed)
                            {
                                this.Conectar();
                            }

                            this.comando.CommandText = MicrocolsaDatosCorrespondencia.GenerarSentenciaMySqlStatic(this.comando.CommandText);
                            this.comando.ExecuteNonQuery();
                        }
                        break;
                    case tipoMotorDb.ORACLE10G:
                        {
                            /*
                            if (this.oraConexion.State == ConnectionState.Closed)
                            {
                                this.Conectar();
                            }

                            this.oraComando.CommandText = this.GenerarSentenciaOracleStatic(this.oraComando.CommandText);

                            this.oraComando.Prepare();

                            this.oraComando.ExecuteNonQuery();
                            */



                        }
                        break;
                    default:
                        {
                            if (this.conexion.State == ConnectionState.Closed)
                            {
                                this.Conectar();
                            }

                            this.comando.ExecuteNonQuery();

                        }
                        break;
                }







            }
            catch (Exception ex)
            {

                string mensaje = "Descripción: No se pudo ejecutar la sentencia SQL" +
                    "\nOrigen: BaseDatosRadicadorWindows.EjecutarComando()";
                if (this.comando != null)
                {
                    mensaje += "\nSQL: " + this.comando.CommandText;
                }

                /*
            else {
                mensaje += "\nSQL: " + this.oraComando.CommandText;
            }
            */
                mensaje += "\nDescripción: " + ex.Message;
                throw new Exception(mensaje);
            }

        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado
        /// </summary>
        /// <param name="nombreProcedimiento">Nombre del procedimiento</param>
        /// <param name="valoresParametros">Valores de los parámetros del procedimiento de la forma: nombreParametro1,valorParametro1,..,nombreParametroN,valorParametroN</param>

        public void EjecutarProcedimientoAlmacenado(string nombreProcedimiento, string valoresParametros)
        {
            try
            {

                DataSet dsParametros = null;

                string[] arrValoresParametros = valoresParametros.Split('|');


                SortedList<string, string> lstParametroValor = new SortedList<string, string>();

                for (int i = 0; i < arrValoresParametros.GetLength(0); i += 2)
                {
                    lstParametroValor.Add(arrValoresParametros[i].ToUpper(), arrValoresParametros[i + 1]);
                }

                this.Conectar();

                switch (this.motorDb)
                {
                    case tipoMotorDb.ORACLE10G:
                        this.CrearComando(SentenciasOracle.PARAMETROS_PROCEDIMIENTOS_ALMACENADOS);
                        break;
                    case tipoMotorDb.MSSQL:
                        this.CrearComando(SentenciasMsSqlB.PARAMETROS_PROCEDIMIENTOS_ALMACENADOS);

                        break;
                    //throw new Exception("No se ha desarrollado esta funcionalidad para MSSQL");
                    case tipoMotorDb.ORACLEMS:
                        this.CrearComando(SentenciasOracle.PARAMETROS_PROCEDIMIENTOS_ALMACENADOS);
                        //this.CrearComando("SELECT * FROM ALL_ARGUMENTS WHERE OBJECT_NAME = UPPER(@PROCEDIMIENTO) ORDER BY ARGUMENT_NAME");
                        break;
                    case tipoMotorDb.INTERNO:
                        this.CrearComando(SentenciasMsSqlB.PARAMETROS_PROCEDIMIENTOS_ALMACENADOS);
                        break;
                    case tipoMotorDb.MYSQL:
                        this.CrearComando(SentenciasMySqlB.PARAMETROS_PROCEDIMIENTOS_ALMACENADOS);
                        this.AsignarParametroCadena(this.caracterParametro + "BASEDATOS", this.nombreDb);

                        break;
                    default:
                        throw new Exception("No se ha desarrollado esta funcionalidad para: " + this.motorDb.ToString());
                        break;
                }

                string[] arrNombreProcedimiento = nombreProcedimiento.Split('.');
                string nombreProcedimientoParametros = arrNombreProcedimiento[arrNombreProcedimiento.Length - 1];


                this.AsignarParametroCadena(this.caracterParametro + "PROCEDIMIENTO", nombreProcedimientoParametros);

                dsParametros = this.EjecutarConsultaDS("PARAMETROS");

                this.Desconectar();

                if (this.motorDb == tipoMotorDb.MYSQL)
                {
                    if (dsParametros.Tables["PARAMETROS"].Rows.Count > 0)
                    {
                        string strParametrosMySQL = System.Text.Encoding.ASCII.GetString((byte[])dsParametros.Tables["PARAMETROS"].Rows[0]["NOMBRE_ARGUMENTO"]);
                        string[] arrParametrosMySQL = strParametrosMySQL.Split(',');
                        dsParametros.Tables.Clear();

                        DataTable tblParametrosMySQL = new DataTable("PARAMETROS");

                        DataColumn colNombre = new DataColumn("NOMBRE_ARGUMENTO", System.Type.GetType("System.String"));
                        DataColumn colDireccion = new DataColumn("DIRECCION", System.Type.GetType("System.String"));
                        DataColumn colTamano = new DataColumn("TAMANO", System.Type.GetType("System.Int16"));
                        DataColumn colPosicion = new DataColumn("POSICION", System.Type.GetType("System.Int16"));
                        DataColumn colTipoDato = new DataColumn("TIPO_DATO", System.Type.GetType("System.String"));

                        tblParametrosMySQL.Columns.Add(colNombre);
                        tblParametrosMySQL.Columns.Add(colDireccion);
                        tblParametrosMySQL.Columns.Add(colTamano);
                        tblParametrosMySQL.Columns.Add(colPosicion);
                        tblParametrosMySQL.Columns.Add(colTipoDato);



                        for (int j = 0; j < arrParametrosMySQL.Length; j++)
                        {
                            DataRow drow = tblParametrosMySQL.NewRow();
                            arrParametrosMySQL[j] = arrParametrosMySQL[j].Trim();
                            string[] arrParametroMySQLB = arrParametrosMySQL[j].Split(' ');
                            string tamanoMySQL = "";
                            if (arrParametroMySQLB.Length == 2)
                            {
                                drow["NOMBRE_ARGUMENTO"] = arrParametroMySQLB[0];
                                drow["DIRECCION"] = "";
                                tamanoMySQL = arrParametroMySQLB[1];
                            }
                            else
                            {
                                drow["NOMBRE_ARGUMENTO"] = arrParametroMySQLB[1];
                                drow["DIRECCION"] = arrParametroMySQLB[0];
                                tamanoMySQL = arrParametroMySQLB[2];
                            }

                            drow["POSICION"] = j + 1;
                            int posTamanoMySQL1 = tamanoMySQL.IndexOf('(');
                            string tamanoMySQLB = "";
                            if (posTamanoMySQL1 != -1)
                            {
                                tamanoMySQLB = tamanoMySQL.Substring(posTamanoMySQL1 + 1, tamanoMySQL.Length - posTamanoMySQL1 - 2);
                                drow["TIPO_DATO"] = tamanoMySQL.Substring(0, posTamanoMySQL1);
                            }
                            else
                            {
                                tamanoMySQLB = "0";
                                drow["TIPO_DATO"] = tamanoMySQL;
                            }
                            //string tamanoMySQLB = tamanoMySQL.Substring(posTamanoMySQL1 + 1, tamanoMySQL.Length- posTamanoMySQL1-2);
                            drow["TAMANO"] = Int16.Parse(tamanoMySQLB);
                            //drow["TIPO_DATO"] = tamanoMySQL.Substring(0, posTamanoMySQL1);

                            tblParametrosMySQL.Rows.Add(drow);
                        }

                        dsParametros.Tables.Add(tblParametrosMySQL);


                    }
                }

                this.CrearComando(nombreProcedimiento, CommandType.StoredProcedure);

                if (this.motorDb == tipoMotorDb.MSSQL)
                {
                    this.sqlComando = new SqlCommand();
                }

                for (int k = 0; k < arrValoresParametros.GetLength(0); k += 2)
                {
                    DataRow dr = null;
                    foreach (DataRow drArgumento in dsParametros.Tables[0].Rows)
                    {

                        string nombreArgumentoA = drArgumento["NOMBRE_ARGUMENTO"].ToString();

                        string nombreArgumentoB = arrValoresParametros[k];

                        switch (this.motorDb)
                        {
                            case tipoMotorDb.MSSQL:

                                if (!nombreArgumentoA.StartsWith("@"))
                                {
                                    nombreArgumentoA = "@" + nombreArgumentoA;
                                }

                                if (!nombreArgumentoB.StartsWith("@"))
                                {
                                    nombreArgumentoB = "@" + nombreArgumentoB;
                                }

                                break;
                            default:
                                break;
                        }



                        if (nombreArgumentoA.ToUpper().Equals(nombreArgumentoB.ToUpper()))
                        {
                            dr = drArgumento;
                            break;
                        }
                    }

                    if (dr != null)
                    {
                        switch (this.motorDb)
                        {
                            case tipoMotorDb.ORACLE10G:
                                {

                                }
                                break;
                            case tipoMotorDb.ORACLEMS:
                                {
                                    #region OracleMS
                                    //System.Data.OracleClient.OracleParameter parametro = null;

                                    DbParameter parametro = null;
                                    //OracleParameter parametro = null;

                                    ParameterDirection direccion = ParameterDirection.Input;
                                    //OracleType tipo = OracleType.Number;
                                    System.Data.OracleClient.OracleType tipo = System.Data.OracleClient.OracleType.Int16;
                                    //DbType tipo = DbType.Int16;

                                    object valor = null;

                                    string strDireccion = dr["DIRECCION"].ToString();

                                    switch (strDireccion)
                                    {
                                        case "OUT":
                                            direccion = ParameterDirection.Output;
                                            break;
                                        case "IN":
                                            direccion = ParameterDirection.Input;
                                            break;
                                        default:
                                            direccion = ParameterDirection.InputOutput;
                                            break;

                                    }

                                    switch (dr["TIPO_DATO"].ToString().ToUpper())
                                    {
                                        case "NUMBER":
                                            //tipo = DbType.Int16;

                                            tipo = System.Data.OracleClient.OracleType.Number;

                                            if ((direccion == ParameterDirection.Input) || (direccion == ParameterDirection.InputOutput))
                                            {
                                                if (lstParametroValor[dr["NOMBRE_ARGUMENTO"].ToString()].Length > 0)
                                                {
                                                    valor = int.Parse(lstParametroValor[dr["NOMBRE_ARGUMENTO"].ToString().ToUpper()]);
                                                }
                                                else
                                                {
                                                    valor = 0;
                                                }
                                            }
                                            else
                                            {
                                                valor = double.Parse("0");
                                                //valor = float.Parse("0") ;
                                                //valor = null;
                                                //valor = "";
                                                //valor = new  System.Data.OracleClient.OracleType();
                                                //System.Data.OracleClient.OracleType ot = new OracleType();     




                                            }


                                            break;
                                        case "VARCHAR2":
                                            tipo = System.Data.OracleClient.OracleType.VarChar;

                                            switch (direccion)
                                            {
                                                case ParameterDirection.Input:
                                                    {
                                                        if (lstParametroValor[dr["NOMBRE_ARGUMENTO"].ToString().ToUpper()] != null)
                                                        {
                                                            valor = lstParametroValor[dr["NOMBRE_ARGUMENTO"].ToString().ToUpper()];

                                                            if (((string)valor).Length == 0)
                                                            {
                                                                valor = "";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            valor = "";
                                                        }
                                                    }
                                                    break;
                                                case ParameterDirection.Output:
                                                    {
                                                        valor = "                                                                                                    ";
                                                    }
                                                    break;
                                                case ParameterDirection.InputOutput:
                                                    {

                                                        if (lstParametroValor[dr["NOMBRE_ARGUMENTO"].ToString().ToUpper()] != null)
                                                        {
                                                            valor = lstParametroValor[dr["NOMBRE_ARGUMENTO"].ToString().ToUpper()];

                                                            if (((string)valor).Length == 0)
                                                            {
                                                                valor = "";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            valor = "";
                                                        }

                                                    }
                                                    break;
                                            }


                                            /*
                                            if ((direccion == ParameterDirection.Input) || (direccion == ParameterDirection.InputOutput))
                                            {
                                                if (lstParametroValor[dr["NOMBRE_ARGUMENTO"].ToString().ToUpper()] != null)
                                                {
                                                    valor = lstParametroValor[dr["NOMBRE_ARGUMENTO"].ToString().ToUpper()];

                                                    if (((string)valor).Length == 0) {
                                                        valor = "                                                                                                    ";
                                                    }
                                                }
                                                else {
                                                    valor = "                                                                                                    ";
                                                }
                                            }
                                            else
                                            {
                                                
                                                valor = "                                                                                                    ";
                                            }
                                            */
                                            break;
                                        default:
                                            break;

                                    }



                                    //parametro = new OracleParameter(dr["NOMBRE_ARGUMENTO"].ToString(), tipo, 50);

                                    parametro = this.comando.CreateParameter();

                                    parametro.ParameterName = dr["NOMBRE_ARGUMENTO"].ToString();

                                    parametro.Direction = direccion;

                                    parametro.Value = valor;

                                    this.comando.Parameters.Add(parametro);
                                    #endregion OracleMS
                                }
                                break;
                            case tipoMotorDb.MSSQL:
                                {
                                    #region MsSql
                                    DbParameter parametro = null;
                                    //OracleParameter parametro = null;

                                    ParameterDirection direccion = ParameterDirection.Input;
                                    //OracleType tipo = OracleType.Number;
                                    //DbType tipo = DbType.Int16;

                                    SqlDbType tipo = SqlDbType.Int;

                                    object valor = null;

                                    switch (dr["DIRECCION"].ToString())
                                    {
                                        case "1":
                                            direccion = ParameterDirection.Output;
                                            break;
                                        case "0":
                                            direccion = ParameterDirection.Input;
                                            break;
                                        default:
                                            direccion = ParameterDirection.InputOutput;
                                            break;

                                    }

                                    switch (dr["TIPO_DATO"].ToString().ToUpper())
                                    {
                                        case "INT":
                                            tipo = SqlDbType.Int;
                                            if (direccion == ParameterDirection.Input)
                                            {
                                                valor = int.Parse(lstParametroValor[dr["NOMBRE_ARGUMENTO"].ToString().ToUpper()]);
                                            }
                                            else
                                            {
                                                valor = 0;
                                            }


                                            break;

                                        case "VARCHAR":
                                            tipo = SqlDbType.VarChar;
                                            if (direccion == ParameterDirection.Input)
                                            {
                                                valor = lstParametroValor[dr["NOMBRE_ARGUMENTO"].ToString().ToUpper()];
                                            }
                                            else
                                            {
                                                valor = "";

                                            }
                                            break;

                                        case "DATETIME":
                                            tipo = SqlDbType.DateTime;
                                            if (direccion == ParameterDirection.Input)
                                            {
                                                valor = lstParametroValor[dr["NOMBRE_ARGUMENTO"].ToString().ToUpper()];
                                            }
                                            else
                                            {
                                                valor = "";

                                            }
                                            break;

                                        default:

                                            tipo = SqlDbType.VarChar;
                                            if (direccion == ParameterDirection.Input)
                                            {
                                                valor = lstParametroValor[dr["NOMBRE_ARGUMENTO"].ToString().ToUpper()];
                                            }
                                            else
                                            {
                                                valor = "";

                                            }

                                            break;

                                    }


                                    this.sqlComando.Parameters.Add(dr["NOMBRE_ARGUMENTO"].ToString(), tipo, int.Parse(dr["TAMANO"].ToString()));

                                    //parametro = new OracleParameter(dr["NOMBRE_ARGUMENTO"].ToString(), tipo, 50);

                                    //this.sqlComando.Parameters.Add(dr["NOMBRE_ARGUMENTO"].ToString(), SqlDbType.Variant);
                                    this.sqlComando.Parameters[dr["NOMBRE_ARGUMENTO"].ToString()].Direction = direccion;
                                    this.sqlComando.Parameters[dr["NOMBRE_ARGUMENTO"].ToString()].Value = valor;

                                    //this.sqlComando.Parameters.Add(valor);
                                    //this.sqlComando.Parameters[dr["NOMBRE_ARGUMENTO"].ToString()].Direction = direccion;
                                    //this.sqlComando.Parameters[dr["NOMBRE_ARGUMENTO"].ToString()].Value = valor;


                                    //parametro = this.comando.CreateParameter();

                                    //parametro.ParameterName = dr["NOMBRE_ARGUMENTO"].ToString();

                                    //parametro.Direction = direccion;

                                    //parametro.Value = valor;



                                    //this.comando.Parameters.Add(parametro);
                                    #endregion Mssql
                                }
                                break;

                            case tipoMotorDb.INTERNO:
                                {
                                    #region MsSql
                                    DbParameter parametro = null;
                                    //OracleParameter parametro = null;

                                    ParameterDirection direccion = ParameterDirection.Input;
                                    //OracleType tipo = OracleType.Number;
                                    DbType tipo = DbType.Int16;

                                    object valor = null;

                                    switch (dr["DIRECCION"].ToString())
                                    {
                                        case "1":
                                            direccion = ParameterDirection.Output;
                                            break;
                                        case "0":
                                            direccion = ParameterDirection.Input;
                                            break;
                                        default:
                                            direccion = ParameterDirection.InputOutput;
                                            break;

                                    }

                                    switch (dr["TIPO_DATO"].ToString().ToUpper())
                                    {
                                        case "INT":
                                            tipo = DbType.Int16;
                                            if (direccion == ParameterDirection.Input)
                                            {
                                                valor = int.Parse(lstParametroValor[dr["NOMBRE_ARGUMENTO"].ToString().ToUpper()]);
                                            }
                                            else
                                            {
                                                valor = 0;
                                            }


                                            break;
                                        default:
                                            tipo = DbType.String;
                                            if (direccion == ParameterDirection.Input)
                                            {
                                                valor = lstParametroValor[dr["NOMBRE_ARGUMENTO"].ToString().ToUpper()];
                                            }
                                            else
                                            {
                                                valor = "                                          ";
                                            }

                                            break;

                                    }



                                    //parametro = new OracleParameter(dr["NOMBRE_ARGUMENTO"].ToString(), tipo, 50);

                                    parametro = this.comando.CreateParameter();

                                    parametro.ParameterName = dr["NOMBRE_ARGUMENTO"].ToString();

                                    parametro.Direction = direccion;

                                    parametro.Value = valor;

                                    this.comando.Parameters.Add(parametro);
                                    #endregion Mssql
                                }

                                break;
                            default:
                                {
                                    #region MySql
                                    DbParameter parametro = null;
                                    //OracleParameter parametro = null;

                                    ParameterDirection direccion = ParameterDirection.Input;
                                    //OracleType tipo = OracleType.Number;
                                    DbType tipo = DbType.Int16;

                                    object valor = null;

                                    switch (dr["DIRECCION"].ToString())
                                    {
                                        case "OUT":
                                            direccion = ParameterDirection.Output;
                                            break;
                                        case "":
                                            direccion = ParameterDirection.Input;
                                            break;
                                        default:
                                            direccion = ParameterDirection.InputOutput;
                                            break;

                                    }

                                    switch (dr["TIPO_DATO"].ToString().ToUpper())
                                    {
                                        case "NUMERIC":
                                            tipo = DbType.Int16;
                                            if (direccion == ParameterDirection.Input)
                                            {
                                                valor = int.Parse(lstParametroValor[dr["NOMBRE_ARGUMENTO"].ToString().ToUpper()]);
                                            }
                                            else
                                            {
                                                valor = 0;
                                            }


                                            break;

                                        case "INTEGER":
                                            tipo = DbType.Int16;
                                            if (direccion == ParameterDirection.Input)
                                            {
                                                valor = int.Parse(lstParametroValor[dr["NOMBRE_ARGUMENTO"].ToString().ToUpper()]);
                                            }
                                            else
                                            {
                                                valor = 0;
                                            }


                                            break;

                                        default:
                                            tipo = DbType.String;
                                            if ((direccion == ParameterDirection.Input) || (direccion == ParameterDirection.InputOutput))
                                            {
                                                valor = lstParametroValor[dr["NOMBRE_ARGUMENTO"].ToString().ToUpper()];
                                            }
                                            else
                                            {
                                                valor = "                                          ";
                                            }

                                            break;

                                    }



                                    //parametro = new OracleParameter(dr["NOMBRE_ARGUMENTO"].ToString(), tipo, 50);

                                    parametro = this.comando.CreateParameter();

                                    parametro.ParameterName = dr["NOMBRE_ARGUMENTO"].ToString();

                                    parametro.Direction = direccion;

                                    parametro.Value = valor;

                                    this.comando.Parameters.Add(parametro);
                                    #endregion MySql
                                }
                                //throw new Exception("No se ha desarrollado esta funcionalidad");
                                break;

                        }
                    }
                }

                switch (this.motorDb)
                {
                    case tipoMotorDb.MSSQL:
                        SqlConnection cnSql = new SqlConnection(this.conexion.ConnectionString);
                        this.sqlComando.Connection = cnSql;
                        this.sqlComando.CommandType = CommandType.StoredProcedure;
                        this.sqlComando.CommandText = nombreProcedimiento;
                        this.sqlComando.Connection = cnSql;
                        this.sqlComando.CommandTimeout = 3600;
                        cnSql.Open();
                        this.sqlComando.ExecuteNonQuery();
                        cnSql.Close();
                        break;
                    default:
                        this.Conectar();
                        this.EjecutarComando();
                        this.Desconectar();
                        break;
                }




            }
            catch (Exception ex)
            {
                string mensaje = "Descripción no se pudo ejecutar el procedimiento almacenado: " + nombreProcedimiento +
                    "\nDescripción: " + ex.Message +
                    "\nOrigen: MicrocolsaDatosCorrespondencia.EjecutarProcedimientoAlmacenado()";

                throw new Exception(mensaje);

            }
        }

        /// <summary>
        /// Guarda un arreglo de bytes  en Base de datos
        /// </summary>
        /// <param name="NombreTabla">Nombre de la tabla</param>
        /// <param name="NombreCampoBinario">Nombre del campo binario</param>
        /// <param name="NombreCampoBusqueda">Nombre del campo de busqueda</param>
        /// <param name="ValorCampoBusqueda">Valor para buscar</param>
        /// <param name="BLOB">Arreglo de bytes a guardar</param>
        public void GuardarBytesDB(string NombreTabla, string NombreCampoBinario, string NombreCampoBusqueda, string ValorCampoBusqueda, byte[] BLOB)
        {
            try
            {

                switch (this.motorDb)
                {
                    case tipoMotorDb.ORACLE10G:
                        {
                            throw new Exception("No se ha implemantado esta función para bases de datos ORACLE");
                        }

                        break;
                    default:
                        {
                            comando = factory.CreateCommand();
                            comando.Connection = this.conexion;
                        }

                        break;
                }



                string sqlBusqueda = "SELECT " + NombreCampoBusqueda + " FROM " + NombreTabla + " WHERE " + NombreCampoBusqueda + "='" + ValorCampoBusqueda + "'";
                CrearComando(sqlBusqueda);

                DbDataReader rd = null;


                switch (motorDb)
                {
                    case tipoMotorDb.ORACLE10G:
                        throw new Exception("No se ha implementado esta funcionalidad para ORACLE10G");
                        break;
                    default:
                        rd = (DbDataReader)EjecutarConsulta();
                        break;
                }



                bool insertar = true;
                while (rd.Read())
                {
                    insertar = false;
                    break;
                }
                rd.Close();

                if (insertar)
                {
                    string sqlInsert = "INSERT INTO " + NombreTabla + "(" + NombreCampoBusqueda + ") VALUES('" + ValorCampoBusqueda + "')";
                    CrearComando(sqlInsert);
                    EjecutarComando();
                }


                comando.CommandText = "UPDATE " + NombreTabla +
                     " SET " + NombreCampoBinario + "=" + caracterParametro + NombreCampoBinario + " WHERE " +
                     NombreCampoBusqueda + "=" + caracterParametro + NombreCampoBusqueda;

                DbParameter campoBusqueda = factory.CreateParameter();
                DbParameter campoBinario = factory.CreateParameter();

                campoBusqueda.ParameterName = caracterParametro + NombreCampoBusqueda;
                campoBinario.ParameterName = caracterParametro + NombreCampoBinario;
                campoBusqueda.Value = ValorCampoBusqueda;
                campoBinario.Value = BLOB;


                comando.Parameters.Add(campoBusqueda);
                comando.Parameters.Add(campoBinario);
                comando.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public object LeerBytesDB(string NombreTabla, string NombreCampoBinario, string NombreCampoBusqueda, string ValorCampoBusqueda)
        {
            try
            {

                switch (this.motorDb)
                {
                    case tipoMotorDb.ORACLE10G:
                        throw new Exception("No se ha implementado esta funcionalidad para bases de datos ORACLE");
                        break;
                }

                MemoryStream memStrm = new MemoryStream();
                BinaryFormatter bin = new BinaryFormatter();
                BinaryWriter writer = new BinaryWriter(memStrm);

                DbCommand comando = factory.CreateCommand();
                comando.Connection = this.conexion;



                comando.CommandText = "SELECT " + NombreCampoBinario + " FROM " +
                    NombreTabla + " WHERE " +
                    //NombreCampoBusqueda + "=@" + NombreCampoBusqueda;
                    //NombreCampoBusqueda + "=?" + NombreCampoBusqueda;
                    NombreCampoBusqueda + "=" + caracterParametro + NombreCampoBusqueda;

                DbParameter campoBusqueda = factory.CreateParameter();

                //campoBusqueda.ParameterName = "@" + NombreCampoBusqueda;
                //campoBusqueda.ParameterName = "?" + NombreCampoBusqueda;
                campoBusqueda.ParameterName = caracterParametro + NombreCampoBusqueda;

                campoBusqueda.Value = ValorCampoBusqueda;


                comando.Parameters.Add(campoBusqueda);

                //int longituBuffer = 100;
                //int longituBuffer = 2048;
                //int longituBuffer = 8192;
                //int longituBuffer = 16384;
                int longituBuffer = 32768;
                long posicionActual = 0;
                long bytesRetornados = 0;
                byte[] BLOB = new byte[longituBuffer];

                bool valorNulo = true;

                DbDataReader reader = comando.ExecuteReader();

                while (reader.Read())
                {
                    if (reader[0] != DBNull.Value)
                    {

                        long cantidadBitsCampoBinario = ((byte[])reader[NombreCampoBinario]).LongLength;

                        valorNulo = false;
                        bytesRetornados = reader.GetBytes(0, posicionActual, BLOB, 0, longituBuffer);
                        while (bytesRetornados == longituBuffer)
                        {
                            writer.Write(BLOB);
                            writer.Flush();



                            if ((posicionActual + longituBuffer) < cantidadBitsCampoBinario)
                            {
                                posicionActual += longituBuffer;
                                bytesRetornados = reader.GetBytes(0, posicionActual, BLOB, 0, longituBuffer);
                            }
                            else
                            {
                                posicionActual += longituBuffer;
                                break;
                                /*
                                long totalBitsFinales = cantidadBitsCampoBinario - posicionActual;
                                if (totalBitsFinales <= 0)
                                {
                                    break;
                                }
                                else { 

                                }
                                posicionActual += longituBuffer;
                                bytesRetornados = reader.GetBytes(0, posicionActual, BLOB, 0,(int)( cantidadBitsCampoBinario - posicionActual));
                                 */
                            }




                        }
                        writer.Write(BLOB, 0, (int)bytesRetornados);
                    }

                }
                reader.Close();
                if (valorNulo == true)
                {
                    return null;
                }
                else
                {
                    memStrm.Position = 0;
                    Object objeto = bin.Deserialize(memStrm);
                    return objeto;
                }




            }
            catch (Exception ex)
            {
                string mensajeError = "Descripción: " + ex.Message +
                    "\nOrigen: BaseDatosRadicadorWindows.LeerBytesDB()";
                throw new Exception(mensajeError);
            }

        }

        /// <summary>
        /// Guarda los datos de un DataSet en la base de datos del radicador
        /// </summary>
        /// <param name="ds">DataSet con los registros a guardar o actualizar</param>
        public void GuardarDataSetEnDBRadicador(System.Data.DataSet ds)
        {
            try
            {

                switch (this.motorDb)
                {
                    case tipoMotorDb.ORACLE10G:
                        throw new Exception("No se ha implementado esta funcionalidad para bases de datos ORACLE");
                        break;
                }

                //1 Insertar las columnas nuevas
                DataSet dsCambios = ds.GetChanges(DataRowState.Added);

                if (dsCambios != null)
                {
                    DataTable tablaInsertar = dsCambios.Tables[0];

                    foreach (DataRow dr in tablaInsertar.Rows)
                    {
                        string strSqlA = "INSERT INTO [" + tablaInsertar.TableName + "] (";
                        string strSqlB = " VALUES(";
                        for (int i = 0; i < dr.ItemArray.GetLength(0); i++)
                        {

                            strSqlA += "[" + tablaInsertar.Columns[i].ColumnName + "],";

                            strSqlB += "'" + dr[tablaInsertar.Columns[i].ColumnName].ToString() + "',";
                        }

                        strSqlA = strSqlA.Substring(0, strSqlA.Length - 1);
                        strSqlB = strSqlB.Substring(0, strSqlB.Length - 1);

                        strSqlA += ")";
                        strSqlB += ")";

                        string strSql = strSqlA + strSqlB;

                        this.CrearComando(strSql);

                        this.EjecutarComando();
                    }
                }

                //2 Actualizar columnas modificadas

                DataSet dsUpdate = ds.GetChanges(DataRowState.Modified);

                if (dsUpdate != null)
                {
                    DataTable tablaUpdate = dsUpdate.Tables[0];




                    foreach (DataRow dr in tablaUpdate.Rows)
                    {
                        string strSqlA = "UPDATE [" + tablaUpdate.TableName + "] SET ";
                        string strSqlB = " WHERE ";
                        for (int i = 0; i < dr.ItemArray.GetLength(0); i++)
                        {

                            strSqlA += "[" + tablaUpdate.Columns[i].ColumnName + "]='" + dr[tablaUpdate.Columns[i].ColumnName, DataRowVersion.Current].ToString() + "',";

                            strSqlB += "[" + tablaUpdate.Columns[i].ColumnName + "]='" + dr[tablaUpdate.Columns[i].ColumnName, DataRowVersion.Original].ToString() + "' AND ";
                        }

                        strSqlA = strSqlA.Substring(0, strSqlA.Length - 1);
                        strSqlB = strSqlB.Substring(0, strSqlB.Length - 4);

                        string strSql = strSqlA + strSqlB;

                        this.CrearComando(strSql);

                        this.EjecutarComando();
                    }
                }

                //3 Borrar las columnas


                DataSet dsBorrados = ds.GetChanges(DataRowState.Deleted);

                if (dsBorrados != null)
                {
                    //dsBorrados.AcceptChanges();
                    DataTable tablaBorrar = dsBorrados.Tables[0];

                    foreach (DataRow dr in tablaBorrar.Rows)
                    {
                        string strSqlA = "DELETE FROM [" + tablaBorrar.TableName + "] WHERE ";
                        for (int i = 0; i < tablaBorrar.Columns.Count; i++)
                        {

                            strSqlA += "[" + tablaBorrar.Columns[i].ColumnName + "]='" + dr[tablaBorrar.Columns[i].ColumnName, DataRowVersion.Original].ToString() + "' AND ";
                        }

                        strSqlA = strSqlA.Substring(0, strSqlA.Length - 4);

                        string strSql = strSqlA;

                        this.CrearComando(strSql);

                        this.EjecutarComando();
                    }
                }

                ds.AcceptChanges();





            }
            catch (Exception ex)
            {
                string mensajeError = "Descripción: No se pudo guardar la información del DataSet en la base de datos" +
                    "\nDescripción: " + ex.Message +
                    "Origen: BaseDatosRadicadorWindows.GuardarDataSetEnDBRadicador()";
                throw new Exception(mensajeError);
            }
        }

        /// <summary>
        /// Permite la ejecucion de scripts sql
        /// </summary>
        public void ejecutarScriptsSql()
        {
            #region atributos
            SqlCommand cmd = new SqlCommand();

            #endregion
            try
            {

                switch (this.motorDb)
                {
                    case tipoMotorDb.ORACLE10G:
                        throw new Exception("No se ha implementado esta funcionalidad para bases de datos ORACLE");
                        break;
                }

                cmd = (SqlCommand)conexion.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = _txtQuery;
                SqlDataReader sdr = cmd.ExecuteReader();
                sdr.Close();
            }
            catch (Exception a)
            {
                _txtError = "Se han presentado los siguientes errores al momento de crear o actualizar la base de datos " + "\n Mensaje = " + a.Message;
            }
        }

        /// <summary>
        /// Corrige una sentencia Sql y la convierte en MySql
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static string GenerarSentenciaMySqlStatic(string sql)
        {
            try
            {
                //sql = sql.ToUpper();
                sql = sql.Replace("[DBO].", "");
                sql = sql.Replace("[", "");
                sql = sql.Replace("]", "");
                sql = sql.Replace("DBO.", "");
                sql = sql.Replace("ISNULL", "IFNULL");
                sql = sql.Replace("DEFAULT GETDATE()", "");
                sql = sql.Replace("DEFAULT YEAR(GETDATE())", "");
                sql = sql.Replace("GETDATE()", "CURRENT_TIMESTAMP()");
                sql = sql.Replace("GETDATE()", "");
                sql = sql.Replace("LEN(", "LENGTH(");

                sql = sql.Replace("[dbo].", "");
                sql = sql.Replace("[", "");
                sql = sql.Replace("]", "");
                sql = sql.Replace("dbo.", "");
                sql = sql.Replace("isnull", "ifnull");
                sql = sql.Replace("default getdate()", "");
                sql = sql.Replace("default year(getdate())", "");
                sql = sql.Replace("getdate()", "current_timestamp()");
                sql = sql.Replace("getdate()", "");
                sql = sql.Replace("len(", "length(");
            }
            catch (Exception ex)
            {
                string mensaje = "Descripción: No se pudo corregir la sentencia sql" +
                    "\nDescripción: " + ex.Message +
                    "\nDescripción: MicrocolsaDatosCorrespondencia.GenerarSentenciaMySqlStatic()";

                throw new Exception(mensaje);
            }
            return sql;

        }

        /// <summary>
        /// Corrige una sentencia Sql y la convierte en sentencia compatible con Oracle
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public string GenerarSentenciaOracleStatic(string sql)
        {
            try
            {


                sql = sql.Replace("[", "");
                sql = sql.Replace("]", "");
            }
            catch (Exception ex)
            {
                string mensaje = "Descripción: No se pudo corregir la sentencia sql" +
                    "\nDescripción: " + ex.Message +
                    "\nDescripción: MicrocolsaDatosCorrespondencia.GenerarSentenciaOracleStatic()";

                throw new Exception(mensaje);
            }
            return sql;

        }
        /// <summary>

        /// <summary>
        /// 
        /// 

        /// Consulta si una tabla, procedimiento se encuentra en la base de datos
        /// </summary>
        /// <param name="nombreTabla"></param>
        /// <returns></returns>
        public bool consultarSiUnaObjetoExisteEnBaseDeDatos(string nombreObjeto)
        {
            #region atributos
            System.Data.DataSet ds = new System.Data.DataSet();
            #endregion
            try
            {

                if (this.conexion.State == System.Data.ConnectionState.Closed)
                {
                    this.Conectar();
                }

                switch (this.motorDb)
                {
                    case MicrocolsaDatosCorrespondencia.tipoMotorDb.MSSQL:
                        {
                            this.CrearComando("select * from dbo.sysobjects where name = ('" + nombreObjeto + "')");
                            ds = this.EjecutarConsultaDS("0");
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                existeTabla = true;
                            }
                            else
                            {
                                existeTabla = false;
                            }
                        }
                        break;
                    case MicrocolsaDatosCorrespondencia.tipoMotorDb.MYSQL:
                        {

                        }
                        break;
                    case MicrocolsaDatosCorrespondencia.tipoMotorDb.ORACLE10G:
                        {

                            throw new Exception("No se ha implementado esta funcionalidad para bases de datos ORACLE");

                        }
                        break;
                    case MicrocolsaDatosCorrespondencia.tipoMotorDb.INTERNO:
                        {
                            this.CrearComando("select * from dbo.sysobjects where name = ('" + nombreObjeto + "')");
                            ds = this.EjecutarConsultaDS("0");
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                existeTabla = true;
                            }
                            else
                            {
                                existeTabla = false;
                            }
                        }
                        break;



                }
            }
            catch (Exception ex)
            {
                existeTabla = false;
                string mensajeError = "Descripción: No se pudo realizar la consulta de la tabla" +
                    "\nDescripción: " + ex.Message +
                    "Origen: BaseDatosRadicadorWindows.consultarSiUnaTablaExiste()";
                throw new Exception(mensajeError);
            }
            return existeTabla;

        }

        /// <summary>
        /// Agrega un parametro a un procedimiento almacenado
        /// Deprecated
        /// </summary>
        public void AgregarParametrosProcedimientoOld(string nombreParametro)
        {
            try
            {

                string[] parametros = nombreParametro.Split(',');

                for (int i = 0; i < parametros.GetLength(0); i += 4)
                {




                    switch (this.motorDb)
                    {
                        case tipoMotorDb.ORACLE10G:
                            {
                                /*
                                Oracle.DataAccess.Client.OracleParameter param = null;
                                switch (parametros[i + 1].ToUpper())
                                {
                                    case "NUMBER":

                                        param = new Oracle.DataAccess.Client.OracleParameter(parametros[i], OracleDbType.Decimal);


                                        if (parametros[i + 2].Length > 0)
                                        {
                                            param.Value = int.Parse(parametros[i + 2]);
                                        }
                                        else
                                        {
                                            param.Value = 0;
                                        }
                                        break;
                                    case "VARCHAR2":
                                        param = new Oracle.DataAccess.Client.OracleParameter(parametros[i], OracleDbType.Varchar2);


                                        if (parametros[i + 2].Length > 0)
                                        {
                                            param.Value = parametros[i + 2];
                                        }
                                        else
                                        {
                                            param.Value = "                    ";


                                        }

                                        break;
                                }

                                switch (parametros[i + 3].ToUpper())
                                {
                                    case "IN":
                                        param.Direction = System.Data.ParameterDirection.Input;
                                        break;
                                    case "OUT":
                                        param.Direction = System.Data.ParameterDirection.Output;
                                        break;
                                    case "IN OUT":
                                        param.Direction = System.Data.ParameterDirection.InputOutput;
                                        break;
                                }

                                this.comando.Parameters.Add(param);
                                */
                            }

                            break;
                        default:
                            //param.DbType = DbType.Object;
                            break;
                    }


                }

            }
            catch (Exception ex)
            {
                string mensajeError = "Descripción: No se pudo agregar el parametro del procedimiento almacenado" +
                    "\nDescripción: " + ex.Message +
                    "Origen: BaseDatosRadicadorWindows.AgregarParametrosProcedimiento()";
                throw new Exception(mensajeError);
            }
        }

        /// <summary>
        /// Agrega un parametro a un procedimiento almacenado
        /// </summary>
        /// <param name="dsParametros">DataSet con la información de los parámetros</param>
        public void AgregarParametrosProcedimiento(string nombreParametro, System.Data.DataSet dsParametros)
        {
            try
            {



                string[] parametros = nombreParametro.Split(',');



                for (int i = 0; i < parametros.GetLength(0); i += 4)
                {




                    switch (this.motorDb)
                    {
                        case tipoMotorDb.ORACLE10G:
                            {
                                /*
                                Oracle.DataAccess.Client.OracleParameter param = null;

                                switch (parametros[i + 1].ToUpper())
                                {
                                    case "NUMBER":

                                        param = new Oracle.DataAccess.Client.OracleParameter(parametros[i], OracleDbType.Decimal);


                                        if (parametros[i + 1].Length > 0)
                                        {
                                            param.Value = int.Parse(parametros[i + 2]);
                                        }
                                        else
                                        {
                                            param.Value = 0;
                                        }
                                        break;
                                    case "VARCHAR2":
                                        param = new Oracle.DataAccess.Client.OracleParameter(parametros[i], OracleDbType.Varchar2);


                                        if (parametros[i + 1].Length > 0)
                                        {
                                            param.Value = parametros[i + 2];
                                        }
                                        else
                                        {
                                            param.Value = "                    ";


                                        }

                                        break;
                                }

                                

                                this.comando.Parameters.Add(param);
                                */
                            }

                            break;
                        default:
                            //param.DbType = DbType.Object;
                            break;
                    }


                }

            }
            catch (Exception ex)
            {
                string mensajeError = "Descripción: No se pudo agregar el parametro del procedimiento almacenado" +
                    "\nDescripción: " + ex.Message +
                    "Origen: BaseDatosRadicadorWindows.AgregarParametrosProcedimiento()";
                throw new Exception(mensajeError);
            }
        }
    }
}
