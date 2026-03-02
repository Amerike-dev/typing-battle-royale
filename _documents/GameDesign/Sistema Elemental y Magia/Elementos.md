# SISTEMA ELEMENTAL  
**Game Design Document (GDD)**  
Versión 1.0  

---

## 1. Visión General

El **Sistema Elemental** define la identidad de juego, el estilo estratégico y las relaciones de poder dentro del battle royale.

Cada jugador elige una **afinidad elemental**, la cual determina:

- Enfoque táctico.  
- Fortalezas y debilidades.  
- Interacciones dentro del espectro elemental.  

El sistema se compone de:

- Elementos Fundamentales  
- Elementos Compuestos  
- Elementos Definitivos  
- Sistema de Inversión Elemental  
- Reacciones Elementales  
- Tipos de Daño  
- Status Effects  


# 2. Elementos Fundamentales

Los elementos fundamentales representan las fuerzas primarias del sistema.

## Tierra
- Enfoque defensivo y estructural.  
- Control del terreno.  
- Creación de constructos de piedra.  

## Agua
- Aplicación constante de **Mojar**.  
- Control de zonas.  
- Sinergias masivas con Hielo y Electricidad.  

## Aire
- Movilidad.  
- Amplificación de efectos.  
- Manipulación de dispersión elemental.  

## Fuego
- Daño sostenido.  
- Eliminación de estados.  
- Alta presión ofensiva.  

---

# 3. Elementos Compuestos

Surgen de la combinación de dos elementos fundamentales.

## Hielo (Aire + Agua)
- Control de movimiento.  
- Aplicación de Congelación mediante reacción.  

## Electricidad (Fuego + Aire)
- Daño en cadena.  
- Aplicación de **Stun** mediante hechizos específicos.  

## Lava (Tierra + Fuego)
- Daño de área persistente.  
- Interacción estructural con piedra.  

## Naturaleza (Tierra + Agua)
- Aplicación de **Rooted** y **Veneno**.  
- Creación de constructos orgánicos.  

---

# 4. Elementos Definitivos

## Oscuridad
- Representa la convergencia total del espectro.  
- Encapsula todos los atributos elementales.  
- Simboliza entropía o absorción total.  

## Luz
- Elemento inverso conceptual de Oscuridad.  
- Representa el equilibrio perfecto.  
- Manifestación opuesta al caos absoluto.  



# 5. Sistema de Inversión Elemental

Relaciones de oposición estratégica:

- Aire ↔ Tierra  
- Agua ↔ Fuego  
- Hielo ↔ Lava  
- Naturaleza ↔ Electricidad  
- Obscuridad ↔ Luz

La inversión implica **ventaja situacional**, no superioridad automática.  

---

# 6. Tipos de Daño

Los tipos de daño determinan interacciones y posibles reacciones.

- Agua  
- Aire  
- Calor (Fuego y Lava)  
- Electricidad  
- Hielo  
- Luz  
- Oscuridad  

---

# 7. Status Effects

Las reacciones elementales se determinan por **dos factores aplicados**:

1. Tipo de daño.  
2. Status effect activo.  

## Mojar
- Se aplica con cualquier instancia de daño de tipo Agua.  
- Algunos hechizos de Agua lo aplican aunque no hagan daño.  

## Quemar
- Solo lo aplican ciertos hechizos de Fuego.  
- El daño de Calor por sí mismo no aplica Quemar.  

## Congelar
- Se aplica cuando un objetivo Mojado recibe daño de Hielo.  
- Es una reacción elemental.  

## Rooted
- Aplicado por hechizos específicos de Naturaleza.  
- No depende del tipo de daño.  

## Veneno
- Aplicado por hechizos de Naturaleza.  
- No depende del tipo de daño.  

## Stun
- Aplicado por ciertos hechizos de Electricidad.  
- No depende del tipo de daño.  

---

# 8. Reacciones Elementales

## 8.1 Congelación
**Condición:** Mojar + Daño de Hielo  
**Resultado:** Inmoviliza temporalmente al objetivo.  

---

## 8.2 Ignición Forzada
**Condición:** Rooted + Daño de Calor  
**Resultado:** Aplica automáticamente **Quemar**.  

---

## 8.3 Sobrecarga Hídrica
**Condición:** Múltiples objetivos Mojados + Daño de Electricidad  
**Resultado:** El daño eléctrico se distribuye a todos los objetivos Mojados dentro del área sin reducción de poder.  

---

## 8.4 Evaporación
**Condición:** Mojar + Daño de Calor  
**Resultado:**  
- Elimina Mojar.  
- Elimina Congelación.  
- Genera acumulación de vapor ambiental.  

---

## 8.5 Lluvia Elemental
**Condición:** Evaporación masiva  
**Resultado:**  
- Se genera lluvia en un área.  
- Todas las unidades dentro del aura reciben automáticamente Mojar.  

---

## 8.6 Transmutación Magmática
**Condición:** Lava + Constructos de Piedra  
**Resultado:** Los constructos de piedra se convierten en lava.  

---

## 8.7 Combustión Orgánica
**Condición:** Constructos de Naturaleza + Daño de Calor  
**Resultado:** Reciben daño aumentado de Fuego y Lava.  

---

## 8.8 Tormenta Acelerante
**Condición:** Aire + Objetivo con Quemar  
**Resultado:** El daño periódico de Quemar se duplica mientras reciba daño de Aire.  

---

## 8.9 Fotosíntesis Arcana
**Condición:** Constructos de Naturaleza + Daño de Agua  
**Resultado:** En lugar de recibir daño, se fortalecen o regeneran.  

---

## 8.10 Aislamiento Mineral
**Condición:** Constructos de Piedra + Daño de Electricidad  
**Resultado:** Son inmunes al daño eléctrico.  

---

# 9. Reglas Especiales

- Tierra no posee status effect propio.  
- No todos los elementos generan estados; algunos modifican el entorno.  
- Las reacciones nunca invalidan el sistema de inversión elemental.  
- Luz y Oscuridad no participan en reacciones elementales tradicionales.  

---

# 10. Estructura del Espectro Elemental

El sistema funciona como un espectro circular:

**Fundamentales → Compuestos → Definitivos**

- Fundamentales: polos primarios.  
- Compuestos: intersecciones híbridas.  
- Oscuridad: convergencia total.  
- Luz: reflejo perfecto.  