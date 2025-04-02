$(document).ready(function () {
    var calendar = new FullCalendar.Calendar($('#calendar')[0], {
        initialView: 'dayGridMonth',
        events: function (info, successCallback, failureCallback) {
            // Cargar los eventos desde el servidor
            $.ajax({
                url: '/Horario/GetHorarios', // Ruta del controlador que devuelve los eventos
                dataType: 'json',
                success: function (data) {
                    var events = data.map(function (event) {
                        return {
                            title: event.title, // Título del evento, por ejemplo: "Juan Pérez - Zona 1"
                            start: event.start,  // Fecha y hora de inicio
                            end: event.end,      // Fecha y hora de fin
                            id: event.id,        // ID del evento
                            description: event.description, // Descripción con detalles adicionales (como turno, etc.)
                            extendedProps: {
                                dia: event.dia,           // Días de trabajo del operador
                                operadorId: event.operadorId, // ID del operador
                                operador: event.operador, // Nombre del operador
                                zonaId: event.zonaId,    // ID de la zona
                                zona: event.zona,        // Nombre de la zona
                                turno: event.turno       // Turno del operador
                            }
                        };
                    });
                    successCallback(events);
                }
            });
        },
        eventClick: function (info) {
// Mostrar el

