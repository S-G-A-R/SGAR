$(document).ready(function () {
    var calendar = new FullCalendar.Calendar($('#calendar')[0], {
        initialView: 'dayGridMonth',
        events: function (info, successCallback, failureCallback) {
            // Puedes cargar los eventos desde la base de datos si es necesario
            $.ajax({
                url: '/Calendario/GetHorarios', // Ruta donde obtienes los eventos
                dataType: 'json',
                success: function (data) {
                    var events = data.map(function (event) {
                        return {
                            title: event.title, // Título del evento (por ejemplo, nombre del operador)
                            start: event.start,  // Fecha y hora de inicio
                            end: event.end,      // Fecha y hora de fin
                            id: event.id         // ID para editar o eliminar el evento
                        };
                    });
                    successCallback(events);
                }
            });
        },
        eventClick: function (info) {
            // Abrir el modal con los detalles del evento para edición
            $('#horarioModal').modal('show');
            var event = info.event;

            // Cargar los datos del evento en el formulario del modal
            // Asumiendo que tienes datos como id, operador, zona, turno, días en la base de datos
            $('#horarioForm #Dia').val(event.extendedProps.dia); // Días
            $('#horarioForm #IdOperador').val(event.extendedProps.operadorId); // Operador
            $('#horarioForm #IdZona').val(event.extendedProps.zonaId); // Zona
            $('#horarioForm #Turno').val(event.extendedProps.turno); // Turno
        },
        dateClick: function (info) {
            // Abrir el modal para crear un nuevo evento (horario)
            $('#horarioModal').modal('show');
            // Puedes pre-llenar la fecha en el formulario si lo deseas
            $('#horarioForm input[name="Dia"]').val(info.dateStr);
        }
    });

    calendar.render();
});
