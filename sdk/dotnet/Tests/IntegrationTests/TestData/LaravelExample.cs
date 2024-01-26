using DaggerSDK;

namespace IntegrationTests.TestData;

// taken from: 
public class LaravelExample
{
	public const string RuntimeQuery = """
		{
		container(platform:"linux/amd64"){
		from(address:"php:8.2-apache-buster"){
		withExec(args:["apt-get","update"]){
		withExec(args:["apt-get","install","--yes","git-core"]){
		withExec(args:["apt-get","install","--yes","zip"]){
		withExec(args:["apt-get","install","--yes","curl"]){
		withExec(args:["docker-php-ext-install","pdo","pdo_mysql","mysqli"]){
		withExec(args:["sh","-c","sed -ri -e \u0027s!/var/www/html!/var/www/public!g\u0027 /etc/apache2/sites-available/*.conf"]){
		withExec(args:["sh","-c","sed -ri -e \u0027s!/var/www/!/var/www/public!g\u0027 /etc/apache2/apache2.conf /etc/apache2/conf-available/*.conf"]){
		withExec(args:["a2enmod","rewrite"]){
		stdout
		}
		}
		}
		}
		}
		}
		}
		}
		}
		}
		}
		""";

	public static Task<string> ContainerBuilder(Client client)
		=> client
			.Container(platform: new Platform("linux/amd64"))
			.From("php:8.2-apache-buster")
			.WithExec(["apt-get", "update"])
			.WithExec(["apt-get", "install", "--yes", "git-core"])
			.WithExec(["apt-get", "install", "--yes", "zip"])
			.WithExec(["apt-get", "install", "--yes", "curl"])
			.WithExec(["docker-php-ext-install", "pdo", "pdo_mysql", "mysqli"])
			.WithExec(["sh", "-c", "sed -ri -e 's!/var/www/html!/var/www/public!g' /etc/apache2/sites-available/*.conf"])
			.WithExec(["sh", "-c", "sed -ri -e 's!/var/www/!/var/www/public!g' /etc/apache2/apache2.conf /etc/apache2/conf-available/*.conf"])
			.WithExec(["a2enmod", "rewrite"])
			.Stdout();
}
