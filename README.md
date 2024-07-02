# TGC - MonoGame - TP

[![.NET](https://github.com/tgc-utn/tgc-monogame-tp/actions/workflows/dotnet.yml/badge.svg)](https://github.com/tgc-utn/tgc-monogame-tp/actions/workflows/dotnet.yml)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/63382c4441444632b06d83dcc6dab106)](https://app.codacy.com/gh/tgc-utn/tgc-monogame-tp/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
[![GitHub license](https://img.shields.io/github/license/tgc-utn/tgc-monogame-tp.svg)](https://github.com/tgc-utn/tgc-monogame-tp/blob/master/LICENSE)

[#BuiltWithMonoGame](http://www.monogame.net) and [.NET Core](https://dotnet.microsoft.com)

## Descripción

Proyecto Derby Games para los trabajos prácticos de la asignatura electiva [Técnicas de Gráficos por Computadora](http://tgc-utn.github.io/) (TGC) en la carrera de Ingeniería en Sistemas de Información. Universidad Tecnológica Nacional, Facultad Regional Buenos Aires (UTN-FRBA).

[#DerbyGames](https://docs.google.com/presentation/d/1DuPCLgdBRjZ9WNkqYEqnQdnWyzGRcpvuLChjyT-bMBA/edit#slide=id.g24ed99bf1a4_0_71)

## Configuración del entorno de desarrollo

Los pasos a seguir según su sistema operativo se pueden leer en el siguiente documento [install.md](https://github.com/tgc-utn/tgc-monogame-samples/blob/master/docs/install/install.md).

Afuera del mundo Windows, vas a necesitar la ayudar de [Wine](https://www.winehq.org) para los shaders, por lo menos por [ahora](https://github.com/MonoGame/MonoGame/issues/2167).

Los recursos usados se almacenan utilizando [Git LFS](https://git-lfs.github.com), con lo cual antes de clonar el repositorio les conviene tenerlo instalado así es automático el pull o si ya lo tienen pueden hacer `git lfs pull`.

## Juego

**Objetivo del Juego:**

El objetivo principal en Derby Games es recoger 10 estrellas en menos de 150 segundos, mientras evitas quedarte sin gasolina. Los jugadores deben utilizar su habilidad de conducción y estrategia para maniobrar el vehículo a través del campo de demolición, recoger estrellas y recargar gasolina en las estaciones de servicio. Todo esto, mientras sobreviven a los ataques de enemigos que intentan destruir tu auto.

**Mecánicas del Juego:**

- Estrellas: Debes recoger 10 estrellas dispersas por el campo de juego antes de que se agoten los 150 segundos.
- Gasolina: Mantén tu nivel de gasolina recargándola en las estaciones de servicio. Si te quedas sin gasolina, perderás.
- Enemigos: Los enemigos te quitan vida cuando están cerca de ti. Debes evitarlos o enfrentarlos para sobrevivir.
- Supervivencia: Sobrevive a los ataques de tus enemigos utilizando habilidades y power-ups.

**Power-Ups:**

- Velocidad: Aumenta temporalmente la velocidad de tu vehículo, permitiéndote escapar de enemigos y recoger estrellas más rápido.
- Misiles: Lanza misiles hacia los enemigos para destruirlos y despejar tu camino.
- Ametralladoras: Equipa tu auto con ametralladoras para disparar a los enemigos cercanos y desbloquear nuevos objetivos.

## Integrantes

Francisco Veiga  |  Martin Gomez  | Pedro Baccaro | Luis Pulgar  |
------------     | -----------    |-------------  | -------------|
|<img src="https://github.com/tgc-utn/tgc-utn.github.io/blob/master/images/robotgc.png" height="500"> | 
<img src="https://github.com/tgc-utn/tgc-utn.github.io/blob/master/images/trofeotp.png" height="500"> |
<img src="https://github.com/TinchoG24/2024-1C-3051-DerbyStudios/assets/129139843/66f911a1-baab-456a-a4e5-82f36ff74618" height="500"> |
<img src="https://github.com/tgc-utn/tgc-utn.github.io/blob/master/images/trofeotp.png" height="500"> |


## Capturas

![image](https://github.com/TinchoG24/2024-1C-3051-DerbyStudios/assets/129139843/921a0f79-db50-41c5-8819-c61126163c46)
![image](https://github.com/TinchoG24/2024-1C-3051-DerbyStudios/assets/129139843/293b3e13-5e06-41bf-aa2f-122b44f01c54)
![image](https://github.com/TinchoG24/2024-1C-3051-DerbyStudios/assets/129139843/d512efa4-5974-4bc8-9f99-62a67ed704f3)


## Game Play
[![Watch the video](https://i9.ytimg.com/vi_webp/yzoQpn3tYas/mq1.webp?sqp=CMzfi7QG-oaymwEmCMACELQB8quKqQMa8AEB-AH-CYAC0AWKAgwIABABGDsgRCh_MA8=&rs=AOn4CLAUT0RlBJwpR_ALaQerLx6Eu5GsfQ)](https://youtu.be/yzoQpn3tYas)
